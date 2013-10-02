﻿using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using Ninject;
using DiversityPhone.Interface;
using DiversityPhone.Services;
using System.IO;

namespace DiversityPhone.Services
{
    public static class StorageMigration
    {
        public static void ApplyMigrationIfNecessary()
        {
            MigrateSettingsFromApplicationSettings();

            var profile = App.Kernel.Get<ICurrentProfile>();
            var currentProfile = profile.CurrentProfilePath();

            MigrateDatabase(currentProfile);
            MigrateVocabulary(currentProfile);
            MigrateMultimedia(currentProfile);
        }

        private static void MigrateVocabulary(string currentProfile)
        {
            var sourceLocation = "vocabularyDB.sdf";
            var targetLocation = Path.Combine(currentProfile, "VocabularyDB.sdf");

            MoveFileIfExists(sourceLocation, targetLocation);
        }

        private static void MigrateDatabase(string currentProfile)
        {
            var sourceLocation = "diversityDB.sdf";
            var targetLocation = Path.Combine(currentProfile, DiversityDataContext.DB_FILENAME);

            MoveFileIfExists(sourceLocation, targetLocation);
        }

        private static void MigrateMultimedia(string currentProfile)
        {
            var sourceLocation = "/multimedia/";
            var targetLocation = Path.Combine(currentProfile, MultimediaStorageService.MEDIA_FOLDER);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.DirectoryExists(sourceLocation))
                {
                    foreach (var file in iso.GetFileNames("/multimedia/*"))
                    {
                        var sourcePath = Path.Combine(sourceLocation, file);
                        var targetPath = Path.Combine(targetLocation, file);

                        iso.MoveFile(sourcePath, targetPath);
                    }
                }
            }
        }

        private static void MoveFileIfExists(string sourceLocation, string targetLocation)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(sourceLocation))
                {
                    iso.MoveFile(sourceLocation, targetLocation);
                }
            }
        }

        private static void MigrateSettingsFromApplicationSettings()
        {
            AppSettings settings;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<AppSettings>("Settings", out settings))
            {
                var svc = App.Kernel.Get<ISettingsService>();
                svc.SaveSettings(settings);
                IsolatedStorageSettings.ApplicationSettings.Remove("Settings");
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

    }
}