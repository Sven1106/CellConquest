Feature: Membrane
    A membrane can be connected to multiple cells.
    A membrane can be touched by a player if it is untouched.
    Touching a membrane will affect its connected cells.

    Scenario: Touching an untouched membrane
        Given a membrane is touched by 'noOne'
        When the membrane is touched by 'player1'
        Then the membrane should be marked as touched by 'player1'

    Scenario: Touching an already touched membrane
        Given a membrane is touched by 'player1'
        When the membrane is touched by 'player2'
        Then 'player2' should be presented with an error message