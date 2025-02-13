using HotChocolate.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.FTUE
{
    public interface IFtueListener
    {
        void Init(object data);

        void RegisterElement(IFtueElement element);
        void UnregisterElement(IFtueElement element);
        IFtueElement GetElementToActivate(IFtueStep ftueStep);

        void AddEvent(IFtueEvent evt);
        void RemoveEvent(IFtueEvent evt);
    }

    public class FtueListener<TData, TResult> : MonoBehaviour, IFtueListener, IDisposable where TData : class where TResult : class, IFtueResult
    {
        public delegate void EventStarted(IFtueEvent evt);
        public event EventStarted OnEventStarted;

        public delegate void EventFinished(IFtueEvent evt, TResult result);
        public event EventFinished OnEventFinished;

        public bool EventIsOngoing => triggeredEvents.Count > 0 && isActiveAndEnabled;
        public IFtueEvent OngoingEvent => EventIsOngoing ? triggeredEvents.Peek() : null;

        public bool EventIsTriggered(IFtueEvent evt) => isActiveAndEnabled && triggeredEvents.Contains(evt);
        public bool EventIsPending(IFtueEvent evt) => isActiveAndEnabled && pendingEvents.Contains(evt);

        private List<IFtueEvent> pendingEvents = new List<IFtueEvent>();
        private Queue<IFtueEvent> triggeredEvents = new Queue<IFtueEvent>();

        private CancellationTokenSource ctSource = new CancellationTokenSource();

        private HashSet<IFtueElement> registeredElements = new HashSet<IFtueElement>();

        public TData Data { get; private set; }

        public void Init(TData data)
        {
            Data = data;
        }

        public void Init(object data)
        {
            Init(data as TData);
        }

        public void RegisterElement(IFtueElement element)
        {
            if (!registeredElements.Contains(element))
                registeredElements.Add(element);
        }

        public void UnregisterElement(IFtueElement element)
        {
            registeredElements.Remove(element);
        }

        public IFtueElement GetElementToActivate(IFtueStep step)
        {
            foreach (var element in registeredElements)
            {
                if (element.ShouldActivateFtue(step))
                    return element;
            }

            return null;
        }

        public void AddEvent(IFtueEvent evt)
        {
            evt.Init(Data, this);

            pendingEvents.Add(evt);
            evt.OnTriggered += OnEventTriggered;

            if (evt.ConditionIsTrue())
                OnEventTriggered(evt);
        }

        public void RemoveEvent(IFtueEvent evt)
        {
            pendingEvents.Remove(evt);
            evt.OnTriggered -= OnEventTriggered;
        }

        public void CancelCurrentEvent()
        {
            ctSource.Cancel();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnEventTriggered(IFtueEvent evt)
        {
            if (!triggeredEvents.Contains(evt) && isActiveAndEnabled)
            {
                triggeredEvents.Enqueue(evt);

                if (triggeredEvents.Count == 1 && gameObject.activeInHierarchy)
                {
                    StopAllCoroutines();
                    StartCoroutine(ExecuteAllEvents());
                }
            }
        }

        private IEnumerator ExecuteAllEvents()
        {
            yield return ExecuteAllEventsAsync().AsIEnumerator();
        }

        private async Task ExecuteAllEventsAsync()
        {
            while (triggeredEvents.Count > 0)
            {
                if (!triggeredEvents.Peek().ConditionIsTrue())
                {
                    triggeredEvents.Dequeue();
                    continue;
                }

                OnEventStarted?.Invoke(triggeredEvents.Peek());

                ctSource?.Dispose();
                ctSource = new CancellationTokenSource();

                var result = await triggeredEvents.Peek().Execute((TResult)Activator.CreateInstance(typeof(TResult)), Data, this, ctSource.Token).ConfigureAwait(true);
                var finishedEvent = triggeredEvents.Peek();

                if (!result.Failed)
                {
                    pendingEvents.Remove(triggeredEvents.Peek());
                    triggeredEvents.Peek().OnTriggered -= OnEventTriggered;
                    triggeredEvents.Peek().Dispose();
                }

                triggeredEvents.Dequeue();
                OnEventFinished?.Invoke(finishedEvent, result as TResult);
            }
        }

        private void OnEnable()
        {
            triggeredEvents.Clear();

            foreach (var evt in pendingEvents)
            {
                if (evt.ConditionIsTrue())
                {
                    if (!triggeredEvents.Contains(evt))
                        triggeredEvents.Enqueue(evt);
                }
            }

            if (triggeredEvents.Count > 0)
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteAllEvents());
            }
        }

        private void OnDisable()
        {
            CancelCurrentEvent();
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            foreach (var pendingEvent in pendingEvents)
            {
                pendingEvent.OnTriggered -= OnEventTriggered;
            }

            foreach (var triggeredEvent in triggeredEvents)
            {
                triggeredEvent.OnTriggered -= OnEventTriggered;
            }

            ctSource?.Dispose();
        }

        public void Dispose()
        {
            ctSource?.Dispose();
        }
    }
}
