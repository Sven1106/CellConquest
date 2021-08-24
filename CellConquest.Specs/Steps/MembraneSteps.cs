using System;
using TechTalk.SpecFlow;
using Xunit;

namespace CellConquest.Tests
{
    [Binding]
    public class MembraneSteps
    {
        private readonly Membrane _membrane;
        private Exception _exception;

        public MembraneSteps(Membrane membrane)
        {
            _membrane = membrane;
        }

        [Given(@"a membrane is touched by '(.*)'")]
        public void GivenAMembraneIsTouchedBy(string player)
        {
            _membrane.MarkAsTouchedBy(player);
        }

        [When(@"the membrane is touched by '(.*)'")]
        public void WhenTheMembraneIsTouchedBy(string player)
        {
            try
            {
                _membrane.MarkAsTouchedBy(player);
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        [Then(@"the membrane should be marked as touched by '(.*)'")]
        public void ThenTheMembraneShouldBeMarkedAsTouchedBy(string player)
        {
            Assert.Equal(player, _membrane.TouchedBy);
        }


        [Then(@"'(.*)' should be presented with an error message")]
        public void ThenShouldBePresentedWithAnErrorMessage(string player)
        {
            Assert.Equal(typeof(MembraneAlreadyTouchedException), _exception.GetType());
        }


        [Then(@"all the connected cells should get notified")]
        public void ThenAllTheConnectedCellsShouldGetNotified() { ScenarioContext.StepIsPending(); }
    }
}