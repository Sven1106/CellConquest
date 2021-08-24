Feature: CellMembraneRelation
	A cellMembraneRelation holds the relation between a cell and a membrane.

@mytag
Scenario: Adding a new CellMembraneRelation
	Given a cell and a membrane is created
	But the cellMembraneRelation doesnt exist
	When a cellMembraneRelation is added
	Then the cellMembraneRelation should be searchable
	
Scenario: Adding an existing CellMembraneRelation
	Given a cell and a membrane is created
	And a cellMembraneRelation is added
	When a cellMembraneRelation is added
	Then an error message should be presented
