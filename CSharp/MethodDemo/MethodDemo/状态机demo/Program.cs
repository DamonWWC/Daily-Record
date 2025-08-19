
using System;
using System.Collections.Generic;

namespace 状态机demo
{


    // 定义状态枚举（参考[3](@ref)）
    public enum State
    {
        Idle,
        ArmedHome,
        ArmedAway,
        Alarm
    }

    // 定义事件枚举（参考[1](@ref)）
    public enum Event
    {
        ArmHomeCommand,
        ArmAwayCommand,
        DisarmCommand,
        IntrusionDetected,
        Timeout
    }

    // 状态机上下文（参考[9](@ref)）
    public class AlarmContext
    {
        private State _currentState = State.Idle;
        private readonly Dictionary<State, Dictionary<Event, Action>> _transitions = new Dictionary<State, Dictionary<Event, Action>>();

        public void AddTransition(State state, Event @event, Action action)
        {
            if (!_transitions.ContainsKey(state))
                _transitions[state] = new Dictionary<Event, Action>();
            _transitions[state][@event] = action;
        }

        public void HandleEvent(Event @event)
        {
            if (_transitions.TryGetValue(_currentState, out var actions) && actions.TryGetValue(@event, out var action))
            {
                action?.Invoke();
                _currentState = GetNextState(@event);
            }
            else
            {
                Console.WriteLine($"Invalid event '{@event}' in state {_currentState}");
            }
        }

        private State GetNextState(Event @event)
        {
            // 状态转移逻辑（参考[1](@ref)）
            switch (_currentState)
            {
                case State.Idle:
                    return @event switch
                    {
                        Event.ArmHomeCommand => State.ArmedHome,
                        Event.ArmAwayCommand => State.ArmedAway,
                        _ => _currentState
                    };
                case State.ArmedHome:
                    return @event switch
                    {
                        Event.IntrusionDetected => State.Alarm,
                        Event.DisarmCommand => State.Idle,
                        _ => _currentState
                    };
                case State.ArmedAway:
                    return @event switch
                    {
                        Event.IntrusionDetected => State.Alarm,
                        Event.DisarmCommand => State.Idle,
                        _ => _currentState
                    };
                case State.Alarm:
                    return @event switch
                    {
                        Event.DisarmCommand => State.Idle,
                        Event.Timeout => State.Idle,
                        _ => _currentState
                    };
                default:
                    return _currentState;
            }
        }
    }

    // 动作执行模块（参考[3](@ref)）
    public static class ActionExecutor
    {
        public static void TriggerAlarm()
        {
            Console.WriteLine("触发警报：启动蜂鸣器并发送通知");
        }

        public static void EnterArmedHome()
        {
            Console.WriteLine("进入在家布防模式：监控外部区域");
        }

        public static void EnterArmedAway()
        {
            Console.WriteLine("进入离家布防模式：监控所有区域");
        }

        public static void ResetSystem()
        {
            Console.WriteLine("系统复位：关闭所有警报");
        }
    }

    // 主程序（参考[6](@ref)）
    class Program
    {
        static void Main(string[] args)
        {
            //var context = new AlarmContext();

            //// 配置状态转移（参考[1](@ref)）
            //context.AddTransition(State.Idle, Event.ArmHomeCommand, () => ActionExecutor.EnterArmedHome());
            //context.AddTransition(State.Idle, Event.ArmAwayCommand, () => ActionExecutor.EnterArmedAway());
            //context.AddTransition(State.ArmedHome, Event.IntrusionDetected, ActionExecutor.TriggerAlarm);
            //context.AddTransition(State.ArmedAway, Event.IntrusionDetected, ActionExecutor.TriggerAlarm);
            //context.AddTransition(State.Alarm, Event.DisarmCommand, ActionExecutor.ResetSystem);
            //context.AddTransition(State.Alarm, Event.Timeout, ActionExecutor.ResetSystem);

            //// 模拟事件序列（参考[3](@ref)）
            //context.HandleEvent(Event.ArmHomeCommand);
            ////context.HandleEvent(Event.IntrusionDetected);
            ////context.HandleEvent(Event.Timeout);
            ////context.HandleEvent(Event.DisarmCommand);
            ///

            ProcessPayMent(3);
            List<int> aaa = new List<int>();
            Span<int> ccc = new Span<int>(aaa.ToArray());
            var aa =new  Memory<int>();

            var arr = new byte[10];
            Span<byte> bytes = arr;
            Span<byte> slicedBytes = bytes.Slice(5, 2);
            slicedBytes[0] = 42;
            slicedBytes[1] = 43;

            bytes[2] = 45; // OK


            


        }

        //public int[] TwoWSumOptimized(int[] nums, int target)
        //{

        //}
        static void ProcessPayMent(Money amount)
        {
            Console.WriteLine($"支付金额：{amount.Amount}");
        }
    }
    public readonly struct Money
    {
        public decimal Amount { get; }
        public Money(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.");
            Amount = amount;
        }
        public static implicit operator Money(decimal amount) => new Money(amount);
    }
}
