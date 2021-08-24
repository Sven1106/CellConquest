Feature: Cell
    A cell is connected to multiple membranes.
    A cell can be conquered by a player when he touches the last untouched membrane connected to it.

Scenario: An unconquered cell can be conquered
    Given a cell has 4 membranes connecting to it
    And 3 of the membranes are touched by player1
    And 1 of the membranes are untouched
    When the untouched membrane is touched by player2
    Then the cell should be conquered by player2
