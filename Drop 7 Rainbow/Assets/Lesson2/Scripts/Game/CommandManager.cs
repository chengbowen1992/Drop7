using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Lesson2
{
    public class CommandManager
    {
        public enum ExecuteMode
        {
            eAtOnce,            //立刻执行
            eDelayTime,         //延迟固定时常
            eAfterFinish        //执行完再继续
        }

        public enum ExecuteState
        {
            eEmpty,
            eExecuting,
        }

        public static CommandManager Instance;

        private CommandGroup cmdGroup = new CommandGroup();
        
        static CommandManager()
        {
            Instance = new CommandManager();
        }

        private CommandManager(){}

        public void AppendCommand(CommandBase cmd)
        {
            cmdGroup.AppendCommand(cmd);
        }

        public void Execute(ExecuteMode mode, Action<CommandGroup, bool> onFinish, float delayTime = 0, bool ifAutoClear = true)
        {
            cmdGroup.Execute(mode, onFinish, delayTime, ifAutoClear);
        }

        public void ResetCommand()
        {
            cmdGroup.ResetGroup();
        }

        public class CommandGroup
        {
            public Queue<CommandBase> QueueTodo = new Queue<CommandBase>();
            public HashSet<CommandBase> SetDoing = new HashSet<CommandBase>();
            public Queue<CommandBase> QueueFinish = new Queue<CommandBase>();

            public ExecuteState State { get; private set; } = ExecuteState.eEmpty;
            public ExecuteMode Mode { get; private set; } = ExecuteMode.eAtOnce;
            public int TotalCount { get; private set; } = 0;
            public int FinishCount => QueueFinish.Count;

            public int ExecutingCount => SetDoing.Count;

            public bool IfComplete => TotalCount == FinishCount;
            
            public float Progress => TotalCount == 0 ? 1f : (float)FinishCount / (float)TotalCount;

            private Action<CommandGroup, bool> onComplete;

            public bool IfAutoClear { get; private set; } = true;

            public void AppendCommand(CommandBase cmd)
            {
                QueueTodo.Enqueue(cmd);
            }
            
            public void Execute(ExecuteMode mode,Action<CommandGroup, bool> onFinish,float delayTime = 0,bool ifAutoClear = true)
            {
                Mode = mode;
                onComplete = onFinish;
                IfAutoClear = ifAutoClear;
                TotalCount = QueueTodo.Count;

                switch (Mode)
                {
                    case ExecuteMode.eAtOnce:
                        ExecuteAtOnce();
                        break;
                    case ExecuteMode.eAfterFinish:
                        ExecuteAfterFinish();
                        break;
                    case ExecuteMode.eDelayTime:
                        ExecuteDelayTime(delayTime);
                        break;
                }
            }

            private void ExecuteAtOnce()
            {
                while (QueueTodo.Count > 0)
                {
                    var cmd = QueueTodo.Dequeue();
                    SetDoing.Add(cmd);
                    
                    cmd.Execute(OnCmdFinish);
                }
            }

            private void ExecuteDelayTime(float delayTime)
            {
                
            }

            private void ExecuteAfterFinish()
            {
                ExecuteAfterOne();
            }

            private bool ExecuteAfterOne()
            {
                if (QueueTodo.Count > 0)
                {
                    var cmd = QueueTodo.Dequeue();
                    SetDoing.Add(cmd);
                    
                    cmd.Execute(OnCmdFinish);
                    return true;
                }

                return false;
            }

            private void OnCmdFinish(CommandBase cmd, bool ifSuccess)
            {
                QueueFinish.Enqueue(cmd);
                
                if (IfComplete)
                {
                    if (IfAutoClear)
                    {
                        ResetGroup();
                    }
                    
                    onComplete?.Invoke(this, true);
                }
                else
                {
                    if (Mode == ExecuteMode.eAfterFinish)
                    {
                        ExecuteAfterOne();
                    }
                }
            }

            public void ResetGroup()
            {
                QueueTodo.Clear();
                SetDoing.Clear();
                QueueFinish.Clear();
                State = ExecuteState.eEmpty;
                Mode = ExecuteMode.eAtOnce;
                TotalCount = 0;
            }
        }
    }
}
