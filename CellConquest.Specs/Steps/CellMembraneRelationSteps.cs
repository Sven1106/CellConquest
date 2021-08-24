using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Xunit;

namespace CellConquest.Tests
{
    [Binding]
    public class CellMembraneRelationSteps
    {
        private Cell _cell;
        private Membrane _membrane;
        private Exception _exception;
        
        private readonly CellMembraneRelationHelper _cellMembraneRelationHelper;
        
        public CellMembraneRelationSteps( CellMembraneRelationHelper cellMembraneRelationHelper)
        {
            _cellMembraneRelationHelper = cellMembraneRelationHelper;
        }

        [Given(@"a cell and a membrane is created")]
        public void GivenACellAndAMembraneIsCreated()
        {
            _cell = new Cell();
            _membrane = new Membrane();
        }

        [Given(@"the cellMembraneRelation doesnt exist")]
        public void GivenTheCellMembraneRelationDoesntExist()
        {
            Assert.Equal(new List<Guid>(), _cellMembraneRelationHelper.GetMembraneIds(_cell.Id));
            Assert.Equal(new List<Guid>(), _cellMembraneRelationHelper.GetCellIds(_membrane.Id));
        }
        
        [Given(@"a cellMembraneRelation is added")]
        public void GivenACellMembraneRelationIsAdded()
        {
            _cellMembraneRelationHelper.AddRelation(_cell.Id, _membrane.Id);
        }

        [When(@"a cellMembraneRelation is added")]
        public void WhenACellMembraneRelationIsAdded()
        {
            try
            {
                _cellMembraneRelationHelper.AddRelation(_cell.Id, _membrane.Id);
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        [Then(@"the cellMembraneRelation should be searchable")]
        public void ThenTheCellMembraneRelationShouldBeSearchable()
        {
            Assert.Single(_cellMembraneRelationHelper.GetMembraneIds(_cell.Id));
            Assert.Single(_cellMembraneRelationHelper.GetCellIds(_membrane.Id));
        }


        [Then(@"an error message should be presented")]
        public void ThenAnErrorMessageShouldBePresented()
        {
            Assert.Equal(typeof(CellMembraneRelationAlreadyExistsException), _exception.GetType());
        }
    }
}