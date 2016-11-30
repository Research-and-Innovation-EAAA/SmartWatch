namespace IoTDataReceiver
{
    /*
    interface IProgressObserver
    {
        /// <summary>
        /// Called by ProgressSubject, that the Observer is Registered to.
        /// </summary>
        /// <param name="progress">0-100%; -1 if indeterminate</param>
        void Notify(int progress);
    }

    abstract class ProgressSubject
    {
        private List<IProgressObserver> observers = new List<IProgressObserver>();

        public void RegisterObserver(IProgressObserver observer)
        {
            if (!this.observers.Contains(observer))
                this.observers.Add(observer);
        }

        public void UnregisterObserver(IProgressObserver observer)
        {
            if (this.observers.Contains(observer))
                this.observers.Remove(observer);
        }

        protected void NotifyObservers(int progress)
        {
            Debug.WriteLine("Notifying observers: " + progress);
            foreach (IProgressObserver o in this.observers)
                o.Notify(progress);
        }
    }*/
}
