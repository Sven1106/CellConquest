import { Point, polygonBounds, polygonInPolygon, Line, lineIntersectsPolygon, polygonScale, pointOnPolygon } from 'geometric';
import { Domain } from './types';

export const createCellsAndMembranesForASquareGrid = (
    coordinatesasdas: Domain.Coordinate[]
): { cells: Domain.Cell[]; membranes: Domain.Membrane[] } => {
    // This is tightly coupled to a 4 wall grid.
    let cells: Domain.Cell[] = [];
    let membranes: Domain.Membrane[] = [];
    const points: Point[] = coordinatesToPoints(coordinatesasdas);
    const bound = polygonBounds(points);
    if (!bound) {
        throw new Error('PolygonBound was null');
    }
    const width = bound[1][0] - bound[0][0];
    const height = bound[1][1] - bound[0][1];
    const scaledPolygon = polygonScale(points, 1.001);
    for (let row = 0; row < height; row++) {
        for (let column = 0; column < width; column++) {
            const predictedCoordinatesForCell = getPredictedCellCoordinates(bound[0], column, row);

            if (!polygonInPolygon(predictedCoordinatesForCell, scaledPolygon) || predictedCoordinatesForCell.length !== 4) {
                continue;
            }
            const membranesWithSameCoordinatesAsCell: Domain.Membrane[] = [];
            const newMembranes: Domain.Membrane[] = [];

            for (let index = 0; index < predictedCoordinatesForCell.length; index++) {
                const edge: Line = [
                    predictedCoordinatesForCell[index],
                    predictedCoordinatesForCell[(index + 1) % predictedCoordinatesForCell.length],
                ];
                let membrane: Domain.Membrane | undefined = membranes.find(m =>
                    coordinatesToPoints(m.coordinates).every(y => edge.every(x => x === y))
                );
                if (!membrane) {
                    const isEdgeAnOutline = lineIntersectsPolygon(edge, points) && pointOnPolygon(edge[0], points) && pointOnPolygon(edge[1], points);
                    let markMembraneAsOutline = isEdgeAnOutline;
                    if (markMembraneAsOutline === false) {
                        const coordinatesOfParallelCell = getCoordinatesOfParallelCell(index, edge);
                        const isAnyCoordinateOfParallelCellInvalid = !polygonInPolygon(coordinatesOfParallelCell, scaledPolygon);
                        markMembraneAsOutline = isAnyCoordinateOfParallelCellInvalid;
                    }
                    membrane = { coordinates: pointsToCoordinates(edge), touchedBy: markMembraneAsOutline ? 'board' : 'noOne' };
                    newMembranes.push(membrane);
                }
                membranesWithSameCoordinatesAsCell.push(membrane);
            }
            if (membranesWithSameCoordinatesAsCell.every(membrane => membrane.touchedBy === 'board')) {
                // Skips a cell if it is already captured
                continue;
            }
            cells = [...cells, { coordinates: pointsToCoordinates(predictedCoordinatesForCell), conqueredBy: 'noOne' }];
            membranes = [...membranes, ...newMembranes];
        }
    }
    return { cells, membranes };
};

const coordinatesToPoints = (coordinates: Domain.Coordinate[]): Point[] => coordinates.map(c => [c.x, c.y]);
const pointsToCoordinates = (points: Point[]): Domain.Coordinate[] => points.map(p => ({ x: p[0], y: p[1] }));

const getPredictedCellCoordinates = (topLeftCorner: Point, column: number, row: number): Point[] => {
    const topLeft: Point = [topLeftCorner[0] + column, topLeftCorner[1] + row];
    const topRight: Point = [topLeftCorner[0] + column + 1, topLeftCorner[1] + row];
    const bottomRight: Point = [topLeftCorner[0] + column + 1, topLeftCorner[1] + row + 1];
    const bottomLeft: Point = [topLeftCorner[0] + column, topLeftCorner[1] + row + 1];

    return [topLeft, topRight, bottomRight, bottomLeft];
};
const getCoordinatesOfParallelCell = (wallIndex: number, edge: Line): Point[] => {
    // This is tightly coupled to 4 wall.
    // Since we know walls are iterated from top > right > bottom > left,
    // we can use the index to map to the corresponding parallel cell walls.

    if (wallIndex === 0) {
        return [
            [edge[0][0], edge[0][1] - 1],
            [edge[1][0], edge[1][1] - 1],
            [edge[1][0], edge[1][1]],
            [edge[0][0], edge[0][1]],
        ];
    }
    if (wallIndex === 1) {
        return [
            [edge[0][0], edge[0][1]],
            [edge[0][0] + 1, edge[0][1]],
            [edge[1][0] + 1, edge[1][1]],
            [edge[1][0], edge[1][1]],
        ];
    }
    if (wallIndex === 2) {
        return [
            [edge[1][0], edge[1][1]],
            [edge[0][0], edge[0][1]],
            [edge[0][0], edge[0][1] + 1],
            [edge[1][0], edge[1][1] + 1],
        ];
    }
    if (wallIndex === 3) {
        return [
            [edge[1][0] - 1, edge[1][1]],
            [edge[1][0], edge[1][1]],
            [edge[0][0], edge[0][1]],
            [edge[0][0] - 1, edge[0][1]],
        ];
    }
    throw new Error('Something went horribly wrong when returning coordinates');
};
