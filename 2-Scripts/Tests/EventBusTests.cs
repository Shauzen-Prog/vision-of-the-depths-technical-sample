using NUnit.Framework;
using System;

namespace Tests.EditMode
{
    public class EventBusTests
    {
        private EventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void SubscribeAndPublish_ShouldInvokeHandler()
        {
            // Arrange
            bool called = false;
            var subscription = _eventBus.Subscribe<int>(x => called = true);
            
            // Act
            _eventBus.Publish(42);
            
            Assert.IsTrue(called);
            subscription.Dispose();
        }

        [Test]
        public void UnSubscribe_Dispose_ShouldNotInvokeHandler()
        {
            // Arrange
            bool called = false;
            var subscription = _eventBus.Subscribe<string>(s => called = true);
            
            // Act
            subscription.Dispose();
            _eventBus.Publish("Hola Test");
            
            // Assert
            Assert.IsFalse(called);
        }

        [Test]
        public void Publish_ShouldCallMultipleSubscribers()
        {
            // Arrange
            int calls = 0;
            var sub1 = _eventBus.Subscribe<float>(f => calls++);
            var sub2 = _eventBus.Subscribe<float>(f => calls++);
            
            // Act
            _eventBus.Publish(10f);
            
            // Assert
            Assert.AreEqual(2, calls);
            
            // CleanUp
            sub1.Dispose();
            sub2.Dispose();
        }

        [Test]
        public void Publish_ShouldTriggerOnAnyEventPublish()
        {
            // Arrange
            Type eventType = null;
            object eventData = null;
            _eventBus.OnAnyEventPublished = (type, data) => { eventType = type; eventData = data; };
            
            // Act
            _eventBus.Publish("TestEvent");
            
            // Assert
            Assert.AreEqual(typeof(string), eventType);
            Assert.AreEqual("TestEvent", eventData);
        }
    }
}
