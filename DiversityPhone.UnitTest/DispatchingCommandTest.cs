using DiversityPhone.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DiversityPhone.UnitTest
{
    internal class TestCommand : ICommand
    {
        public bool _Executed, _CanExecute;

        public bool CanExecute(object parameter)
        {
            return _CanExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            _Executed = true;
        }
    }

    [TestClass]
    public class DispatchingCommandTest
    {
        DispatchingCommand<int> Target;
        public DispatchingCommandTest()
        {
            Target = new DispatchingCommand<int>();
        }

        [TestMethod, TestCategory("DispatchingCommand")]
        public void DisabledWithoutCommands()
        {
            Target.Commands = null;

            Assert.IsFalse(Target.CanExecute(null));
        }

        [TestMethod, TestCategory("DispatchingCommand")]
        public void DisabledWithoutSelectedCommand()
        {
            Target.Commands = new Dictionary<int, ICommand>()
            {
                {0, new TestCommand() { _CanExecute = true }}
            };

            Target.CurrentKey = 1;

            Assert.IsFalse(Target.CanExecute(null));
        }

        [TestMethod, TestCategory("DispatchingCommand")]
        public void DispatchesToSelectedCommand()
        {
            var cmd = new TestCommand() { _CanExecute = true };
            Target.Commands = new Dictionary<int, ICommand>()
            {
                {0, cmd}
            };

            Target.CurrentKey = 0;

            Assert.IsTrue(Target.CanExecute(null));
            Assert.IsFalse(cmd._Executed);

            Target.Execute(null);

            Assert.IsTrue(cmd._Executed);

            cmd._CanExecute = false;

            Assert.IsFalse(Target.CanExecute(null));
        }

        [TestMethod, TestCategory("DispatchingCommand")]
        public void PropagatesCanExecuteChanged()
        {
            var cmd = new TestCommand() { _CanExecute = true };
            Target.Commands = new Dictionary<int, ICommand>()
            {
                {0, cmd}
            };

            Target.CurrentKey = 0;

            var changed = false;

            Target.CanExecuteChanged += (s, a) => changed = true;

            cmd._CanExecute = false;
            cmd.RaiseCanExecChanged();

            Assert.IsTrue(changed);
        }
    }
}
