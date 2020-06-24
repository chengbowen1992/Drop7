using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommandUtil
{
    public interface ICommand<T>
    {
        void OnAppend();
        void Execute(Action<T, bool> onComplete);
        void OnExecute();
        void OnComplete(bool ifSuccess);
    }

    public abstract class BaseCommand : ICommand<BaseCommand> 
    {
        public virtual String Description
        {
            get { return "BaseCommand"; }
        }

        private Action<BaseCommand, bool> onCompleteCall;
        
        public virtual void OnAppend()
        {

        }

        public void Execute(Action<BaseCommand, bool> onComplete)
        {
            onCompleteCall = onComplete;
        }

        public virtual void OnExecute()
        {
            
        }

        public void OnComplete(bool ifSuccess)
        {
            onCompleteCall?.Invoke(this, ifSuccess);
        }
    }
    
    public enum GroupExecuteMode
    {
        eAllAtOnce,
        eAfterFinish,
        eNotWait,
    }

    public sealed class CommandGroup
    {
        public enum GroupExecuteState
        {
            eNone,
            eAppending,
            eExecuting,
            eFinish
        }
        
        public int GroupIndex { get; private set; } = -1;
        public Queue<BaseCommand> CmdsTodo = new Queue<BaseCommand>();
        public Queue<BaseCommand> CmdsFinish = new Queue<BaseCommand>();
        public HashSet<BaseCommand> CmdsDoing = new HashSet<BaseCommand>();

        public CommandGroup(GroupExecuteMode mode)
        {
            SetGroupExecuteMode(mode);
        }

        public GroupExecuteMode ExecuteMode { get; private set; } = GroupExecuteMode.eAllAtOnce;
        
        public GroupExecuteState ExecuteState { get; private set; } = GroupExecuteState.eNone;

        public int TotalCount { get; private set; } = 0;
        public int FinishCount => CmdsFinish.Count;

        public int ExecutingCount => CmdsDoing.Count;

        public bool IfComplete => TotalCount == FinishCount;
            
        public float Progress => TotalCount == 0 ? 1f : (float)FinishCount / (float)TotalCount;

        public bool IfAutoClear { get; private set; } = true;

        private Action<CommandGroup, bool> onCompleteCall;

        public void SetGroupExecuteMode(GroupExecuteMode mode)
        {
#if UNITY_EDITOR
            Debug.Log($"CommandGroup == SetGroupExecuteMode {mode}");
#endif
            if (ExecuteState == GroupExecuteState.eNone)
            {
                ExecuteMode = mode;
            }
            else
            {
                Debug.LogError($"CommandGroup == Can't SetGroupExecuteMode in State: {ExecuteState}");
            }
        }

        public void AppendCommand(BaseCommand command)
        {
            if (ExecuteMode == GroupExecuteMode.eAllAtOnce)
            {
                Debug.LogError($"CommandGroup == NotWaitMode Execute use ExecuteNotWait !!!");
                return;
            }

            if (ExecuteState == GroupExecuteState.eNone || ExecuteState == GroupExecuteState.eAppending)
            {
#if UNITY_EDITOR
                Debug.Log($"CommandGroup == Append {command.Description}");
#endif
                CmdsTodo.Enqueue(command);
            }
            else
            {
                Debug.LogError($"CommandGroup == CanNotAppend from State {ExecuteState}");
            }

        }
        
        public void ExecuteGroup(Action<CommandGroup, bool> onComplete, bool ifAutoClear = true)
        {
#if UNITY_EDITOR
            Debug.Log($"CommandGroup == ExecuteGroup {ExecuteMode} : Todo {TotalCount} & ifAutoClear {ifAutoClear}");
#endif

            if (ExecuteMode == GroupExecuteMode.eNotWait)
            {
                Debug.LogError($"CommandGroup == NotWaitMode Execute use ExecuteNotWait !!!");
                return;
            }

            if (ExecuteState == GroupExecuteState.eNone)
            {
                ExecuteState = GroupExecuteState.eFinish;
                onComplete?.Invoke(this, true);
                return;
            }
            else if (ExecuteState == GroupExecuteState.eAppending)
            {
                ExecuteState = GroupExecuteState.eExecuting;

                onCompleteCall = onComplete;
                IfAutoClear = ifAutoClear;
                TotalCount = CmdsTodo.Count;
                
                switch (ExecuteMode)
                {
                    case GroupExecuteMode.eAllAtOnce:
                        ExecuteAtOnce();
                        break;
                    case GroupExecuteMode.eAfterFinish:
                        ExecuteAfterOne();
                        break;
                }
            }
            else
            {
                Debug.LogError($"CommandGroup == CanNotExecute from State {ExecuteState}");
            }
        }

        public void ExecuteNotWait(BaseCommand command, Action<BaseCommand, bool> onComplete = null)
        {
            command.Execute(onComplete);
        }

        private void ExecuteAtOnce()
        {
            while (CmdsTodo.Count > 0)
            {
                var cmd = CmdsTodo.Dequeue();
                CmdsDoing.Add(cmd);
                cmd.Execute(OnCmdFinish);
            }
        }
        
        private bool ExecuteAfterOne()
        {
            if (CmdsTodo.Count > 0)
            {
                var cmd = CmdsTodo.Dequeue();
                CmdsDoing.Add(cmd);
                    
                cmd.Execute(OnCmdFinish);
                return true;
            }

            return false;
        }
        
        private void OnCmdFinish(BaseCommand cmd, bool ifSuccess)
        {
#if UNITY_EDITOR
            Debug.Log(
                $"CommandGroup == OnCommandFinish {cmd.Description}  finished:{CmdsFinish.Count + 1}&last:{CmdsDoing.Count - 1}");
#endif
            if (CmdsDoing.Remove(cmd))
            {
                CmdsFinish.Enqueue(cmd);
                
                if (IfComplete)
                {
                    ExecuteState = GroupExecuteState.eFinish;

                    if (IfAutoClear)
                    {
                        ResetGroup();
                    }

                    onCompleteCall?.Invoke(this, true);
                    onCompleteCall = null;
                }
                else
                {
                    if (ExecuteMode == GroupExecuteMode.eAfterFinish)
                    {
                        ExecuteAfterOne();
                    }
                }
            }
        }
        
        public void ResetGroup()
        {
            CmdsTodo.Clear();
            CmdsFinish.Clear();
            CmdsDoing.Clear();
            ExecuteState = GroupExecuteState.eNone;
            ExecuteMode = GroupExecuteMode.eAllAtOnce;
            TotalCount = 0;
        }
    }

    public sealed class CommandManager
    {
        public enum ManagerState
        {
            eEmpty,
            eExecuting,
        }
        
        public static CommandManager Instance = new CommandManager();

        private CommandManager()
        {
            DefaultGroup = new CommandGroup(GroupExecuteMode.eNotWait);
        }
        
        public CommandGroup DefaultGroup;
        
        public Queue<CommandGroup> GroupsTodo = new Queue<CommandGroup>();
        public CommandGroup GroupDoing;

        public ManagerState CurrentState { get; private set; }

        private Action<bool> onCompleteCall; 
        
        public void ExecuteNotWait(BaseCommand command, Action<BaseCommand, bool> onComplete = null)
        {
            DefaultGroup.ExecuteNotWait(command, onComplete);
        }

        public void Execute(Action<bool> onComplete)
        {
            if (CurrentState == ManagerState.eEmpty)
            {
                CurrentState = ManagerState.eExecuting;
                onCompleteCall = onComplete;
                ExecuteAfterOne();
            }
            else
            {
                Debug.LogError($"CommandManager == Can't Execute in State:{CurrentState}");
            }
        }

        private bool ExecuteAfterOne()
        {
            if (GroupsTodo.Count > 0)
            {
                GroupDoing = GroupsTodo.Dequeue();
                GroupDoing.ExecuteGroup(OnGroupFinish);
                return true;
            }

            return false;
        }

        private void OnGroupFinish(CommandGroup group, bool ifSuccess)
        {
            //Complete
            if (GroupsTodo.Count == 0)
            {
                CurrentState = ManagerState.eEmpty;
                onCompleteCall?.Invoke(true);
                onCompleteCall = null;
            }
            else
            {
                ExecuteAfterOne();
            }
        }
    }
}