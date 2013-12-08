﻿using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;


namespace DiversityPhone.ViewModels
{

    public class SetupVM : ReactiveObject, IPageServices<IUseCommunication>
    {
        public IUseCommunication Services { get; private set; }
        private readonly IDiversityServiceClient Repository;
        private readonly ISettingsService Settings;

        private IEnumerator<AppSettings> LatestLogin, LatestLoginWithRepo, LatestLoginWithProfile;

        public IReactiveCommand ShowLogin { get; set; }
        public ReactiveAsyncCommand GetRepositories { get; private set; }
        public ReactiveAsyncCommand GetProjects { get; private set; }
        public ReactiveAsyncCommand GetProfile { get; private set; }
        public ReactiveAsyncCommand Save { get; private set; }

        private string NoRepo = DiversityResources.Setup_Item_PleaseChoose;
        public IListSelector<string> Database { get; private set; }

        private Project NoProject = new Project() { DisplayText = DiversityResources.Setup_Item_PleaseChoose, ProjectID = -1 };
        public IListSelector<Project> Project { get; private set; }

        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set { this.RaiseAndSetIfChanged(x => x.UserName, ref _UserName, value); }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
            set { this.RaiseAndSetIfChanged(x => x.Password, ref _Password, value); }
        }

        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;
        public bool IsOnlineAvailable
        {
            get { return _IsOnlineAvailable.Value; }
        }

        private bool _UseGPS = true;
        public bool UseGPS
        {
            get { return _UseGPS; }
            set { _UseGPS = value; }
        }


        private IObservable<Tuple<AppSettings, IList<string>>> GetRepositoriesObservable(object _)
        {
            var user = UserName;
            var pass = Password;
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                // Invalid Argument
                return Observable.Empty<Tuple<AppSettings, IList<string>>>();
            }

            var settings = new AppSettings()
            {
                UserName = user,
                Password = pass
            };

            return Repository.GetRepositories(settings.ToCreds())
                .Select(repos => repos.ToList() as IList<string>)
                .Do(list => list.Insert(0, NoRepo))
                .Select(list => Tuple.Create(settings, list));
        }

        private IObservable<Tuple<AppSettings, IList<Project>>> GetProjectsObservable(object _)
        {
            var repo = Database.SelectedItem;
            var login = LatestLogin.NextOrDefault();

            if (!string.IsNullOrWhiteSpace(repo) && repo != NoRepo && login != null)
            {
                login.HomeDBName = repo;
                return Repository.GetProjectsForUser(login.ToCreds())
                    .Do(list => list.Insert(0, NoProject))
                    .Select(projects => Tuple.Create(login, projects));
            }
            else
            {
                return Observable.Empty<Tuple<AppSettings, IList<Project>>>();
            }
        }

        private IObservable<AppSettings> GetProfileObservable(object _)
        {
            var project = Project.SelectedItem;
            var login = LatestLoginWithRepo.NextOrDefault();

            if (project != null && project != NoProject && login != null)
            {
                login.CurrentProject = project.ProjectID;
                login.CurrentProjectName = project.DisplayText;
                return Repository.GetUserInfo(login.ToCreds())
                    .Select(profile =>
                    {
                        login.AgentName = profile.UserName;
                        login.AgentURI = profile.AgentUri;
                        return login;
                    });
            }
            else
            {
                return Observable.Empty<AppSettings>();
            }
        }

        private void SetLogin(AppSettings settings)
        {
            if (settings != null)
            {
                this.UserName = settings.UserName;
                this.Password = settings.Password;
            }
        }

        private IObservable<Unit> SaveSettings(object _)
        {
            var settings = LatestLoginWithProfile.NextOrDefault();

            if (settings != null)
            {
                settings.UseGPS = this.UseGPS;
                return Observable.Start(() => Settings.SaveSettings(settings));
            }
            else
            {
                return Observable.Empty<Unit>();
            }
        }



        public SetupVM(
            PageVMServices Services,
            ISettingsService Settings,
            IConnectivityService Connectivity,
            IDiversityServiceClient Repository
            )
        {
            this.Repository = Repository;
            this.Settings = Settings;
            this.Services = Services;

            // On First Page Visit (App Launch)
            // If There already is a configuration (Settings)
            // Go To Home Page
            Services.Activation.FirstActivation()
                .Zip(Settings.SettingsObservable(), (_, s) => s != null)
                .Select(x => (x) ? Page.Home : Page.SetupWelcome)
                .ToMessage(Services.Messenger);

            _IsOnlineAvailable = this.ObservableToProperty(Connectivity.WifiAvailable(), x => x.IsOnlineAvailable, false, Services.Dispatcher);

            // Show current login data in case of Reset
            Settings.SettingsObservable()
                .ObserveOn(Services.Dispatcher)
                .Subscribe(SetLogin);

            // Command To begin Setup
            this.ShowLogin = new ReactiveCommand();
            ShowLogin.Select(_ => Page.SetupLogin)
                .ToMessage(Services.Messenger);

            // Command Condition
            var userPassAndWifi =
                Observable.CombineLatest(
                Connectivity.WifiAvailable(),
                this.WhenAny(x => x.UserName, x => x.GetValue()).Select(string.IsNullOrWhiteSpace),
                this.WhenAny(x => x.Password, x => x.GetValue()).Select(string.IsNullOrWhiteSpace),
                (wifi, a, b) => wifi & !(a | b));


            // Command and Errorhandling
            this.GetRepositories = new ReactiveAsyncCommand(userPassAndWifi);
            GetRepositories.ShowInFlightNotification(Services.Notifications, DiversityResources.Setup_Info_ValidatingLogin);
            GetRepositories.ThrownExceptions
                .ShowServiceErrorNotifications(Services.Notifications)
                .ShowErrorNotifications(Services.Notifications)
                .Subscribe();
            var loginAndRepo = GetRepositories.RegisterAsyncObservable(GetRepositoriesObservable).Publish().PermaRef();

            // Page Navigation if Login Successful
            // i.e. Any repositories have been returned
            loginAndRepo
                .Snd()
                .Subscribe(NavigateOrNotifyInvalidCredentials);

            // Repo Selection
            this.Database = new ListSelectionHelper<string>(Services.Dispatcher);
            loginAndRepo
                .Select(t => t.Item2)
                .Merge(EmptyProjectsOnLoginStart())
                .Subscribe(Database.ItemsObserver);

            // Settings Propagation
            LatestLogin = loginAndRepo
               .Fst()
               .MostRecent(null)
               .GetEnumerator();

            // Command Condition
            var repoSelected = Database.SelectedItemObservable
                .Select(repo => repo != NoRepo)
                .AndNoItemsInFlight(GetRepositories);

            // Command and Errorhandling
            this.GetProjects = new ReactiveAsyncCommand(repoSelected);
            GetProjects.ShowInFlightNotification(Services.Notifications, DiversityResources.Setup_Info_GettingProjects);
            GetProjects.ThrownExceptions
                .ShowServiceErrorNotifications(Services.Notifications)
                .ShowErrorNotifications(Services.Notifications)
                .Subscribe();
            var loginAndProjects = GetProjects.RegisterAsyncObservable(GetProjectsObservable).Publish().PermaRef();

            // Page Navigation
            loginAndProjects
                .Select(_ => Page.SetupProject)
                .ToMessage(Services.Messenger);

            // Project Selection
            Project = new ListSelectionHelper<Project>(Services.Dispatcher);
            loginAndProjects
                .Snd()
                .Merge(
                   EmptyReposOnRepoChange()
                   )
                   .Subscribe(Project.ItemsObserver);


            // Settings Propagation
            LatestLoginWithRepo = loginAndProjects
                .Fst()
                .MostRecent(null)
                .GetEnumerator();

            // Command Condition
            var projectSelected = Project.SelectedItemObservable
                .Select(p => p != NoProject)
                .AndNoItemsInFlight(GetProjects);

            // Command and Errorhandling
            this.GetProfile = new ReactiveAsyncCommand(projectSelected);
            GetProfile.ShowInFlightNotification(Services.Notifications, DiversityResources.Setup_Info_GettingProfile);
            GetProfile.ThrownExceptions
                .ShowServiceErrorNotifications(Services.Notifications)
                .ShowErrorNotifications(Services.Notifications)
                .Subscribe();
            var loginWithProfile = GetProfile.RegisterAsyncObservable(GetProfileObservable).Publish().PermaRef();

            // Page Navigation
            loginWithProfile
                .Select(_ => Page.SetupGPS)
                .ToMessage(Services.Messenger);

            // Settings Propagation
            LatestLoginWithProfile = loginWithProfile
                .MostRecent(null)
                .GetEnumerator();

            // Command And Page Navigation
            this.Save = new ReactiveAsyncCommand();
            Save.RegisterAsyncObservable(SaveSettings)
                .Select(_ => Page.SetupVocabulary)
                .ToMessage(Services.Messenger);
        }

        private void NavigateOrNotifyInvalidCredentials(IList<string> repos)
        {
            // Don't count the "NoRepo" Entry
            if (repos != null && repos.Count > 1)
            {
                // Navigate Forward to Database Page
                Services.Messenger.SendMessage(Page.SetupDatabase);
            }
            else
            {
                // Notify user of invalid Credentials
                Services.Notifications.showNotification(DiversityResources.Setup_Info_InvalidCredentials);
            }
        }

        private IObservable<IList<Project>> EmptyReposOnRepoChange()
        {
            return GetProjects.AsyncStartedNotification
                               .Select(_ => new List<Project>() as IList<Project>)
                               .Do(l => l.Add(NoProject));
        }

        private IObservable<IList<string>> EmptyProjectsOnLoginStart()
        {
            return GetRepositories.AsyncStartedNotification
                                .Select(_ => new List<string>() as IList<string>)
                                .Do(l => l.Add(NoRepo));
        }
    }
}
