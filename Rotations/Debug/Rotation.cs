using System.Diagnostics;
using System.Linq;
using Styx;
using Styx.Helpers;
using TreeSharp;
using Action = TreeSharp.Action;

namespace RogueAssassin.Rotations.Debug
{
    internal class Rotation
    {
        public Composite Build()
        {
            return new Decorator(ret => StyxWoW.Me.GotTarget,
                                 new PrioritySelector(
                                     new Action(delegate
                                                    {
                                                        Logging.Write("Starting");
                                                        return Helpers.ResetFail();
                                                    }),
                                     new Decorator(new Action(delegate
                                                       {
                                                           foreach (var aura in StyxWoW.Me.GetAllAuras())
                                                           {
                                                               Logging.WriteDebug("{0} {1} {2}", aura.Name, aura.SpellId, aura.ApplyAuraType);
                                                           }

                                                           return RunStatus.Success;
                                                       })
                                     )));
        }

        private class DebugLog : Action
        {
            private readonly string _message;
            private readonly System.Action _run;

            public DebugLog(string message)
            {
                _message = message;
            }

            public DebugLog(string message, System.Action run)
            {
                _message = message;
                _run = run;
            }

            protected override RunStatus Run(object context)
            {
                if (_run == null)
                    Logging.Write(_message);
                else
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    _run();
                    timer.Stop();

                    Logging.Write("{0}: {1}", _message, timer.ElapsedMilliseconds);
                }

                return RunStatus.Failure;
            }
        }
    }
}