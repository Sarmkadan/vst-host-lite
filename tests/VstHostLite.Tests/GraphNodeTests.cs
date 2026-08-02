using System;
using System.Collections.Generic;
using NUnit.Framework;
using VstHostLite.Native;

namespace VstHostLite.Tests
{
    [TestFixture]
    public class GraphNodeTests
    {
        [Test]
        public void ConstructionTest()
        {
            // Arrange
            var node = new GraphNode("TestNode", 0);

            // Act

            // Assert
            Assert.AreEqual("TestNode", node.Name);
            Assert.AreEqual(0, node.Component);
        }

        [Test]
        public void PropertyDefaultsTest()
        {
            // Arrange
            var node = new GraphNode("TestNode", 0);

            // Act

            // Assert
            Assert.IsNull(node.Prev);
            Assert.IsNull(node.Next);
        }

        [Test]
        public void MutationMethodsTest()
        {
            // Arrange
            var node1 = new GraphNode("Node1", 0);
            var node2 = new GraphNode("Node2", 0);

            // Act
            node1.Next = node2;
            node2.Prev = node1;

            // Assert
            Assert.AreEqual(node2, node1.Next);
            Assert.AreEqual(node1, node2.Prev);
        }
    }
}