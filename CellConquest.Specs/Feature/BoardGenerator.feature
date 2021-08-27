Feature: BoardGenerator
The boardGenerator generates the board consisting of cells and membranes based on a boardlayout.

    @mytag
    Scenario Outline: Generating the size of the board
        Given a <shape> is provided
        When the generation is started
        Then a board with <size> is created
        Examples:
          | shape                               | size      |
          | '1,1 - 2,1 - 2,2 - 1,2'             |     '1,1' |
          | '1,1 - 3,1 - 3,2 - 1,2'             |     '2,1' |
          | '2,1 - 4,1 - 4,2 - 2,2'             |     '2,1' |
          | '2,2 - 4,2 - 4,4 - 2,4'             |     '2,2' |
          | '1,1 - 3,1 - 3,2 - 2,2 - 2,5 - 1,5' |     '2,4' |