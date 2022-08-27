import { trpc } from "../utils/trpc";
import {
  Stage,
  Graphics,
  Text,
  Container,
  Sprite,
  NineSlicePlane,
} from "@inlet/react-pixi";
import { ListenerFn } from "eventemitter3";
import { InteractionEvent } from "pixi.js";
import { useParams } from "react-router-dom";

const GamePage = () => {
  const { gameId } = useParams() as { gameId: string };
  return (
    <>
      <h1>Game: {gameId}</h1>
      <Game gameId={gameId} />
    </>
  );
};
export default GamePage;

const Game = ({ gameId }: { gameId: string }) => {
  const { data, isLoading, error } = trpc.game.byId.useQuery({
    gameId,
  });
  if (isLoading) return <div>Loading...</div>;
  if (error) {
    return <div>Game was not found</div>;
  }
  const onDragMove = (event: any) => {
    const sprite = event.currentTarget;
    if (sprite.dragging) {
      const newPosition = sprite.data!.getLocalPosition(sprite.parent);
      sprite.x = newPosition.x;
      sprite.y = newPosition.y;
    }
  };
  const scale = 100;
  const offset = 0.1;
  return (
    <>
      {JSON.stringify(data)}
      <Stage
        width={500}
        height={500}
        translate="yes"
        options={{
          antialias: true,
          autoDensity: true,
          backgroundColor: 0xffffff,
        }}
      >
        {data.cells.map((cell) => {
          return (
            <>
              <Text
                text="hi"
                x={cell.coordinates[0].x + scale / 2}
                y={cell.coordinates[0].y + scale / 2}
                style={{
                  fontSize: 16,
                }}
              />
              <Graphics
                key={JSON.stringify(cell.coordinates)}
                interactive
                scale={scale}
                draw={(g) => {
                  g.beginFill(Math.random() * 0xffffff, 0.5);
                  g.drawRoundedRect(
                    cell.coordinates[0].x + offset,
                    cell.coordinates[0].y + offset,
                    1,
                    1,
                    0
                  );
                  g.endFill();
                }}
              />
            </>
          );
        })}
        {data.membranes.map((membrane) => {
          const slopeOfMembrane =
            (membrane.coordinates[1].y - membrane.coordinates[0].y) /
            (membrane.coordinates[1].x - membrane.coordinates[0].x);
          console.log(
            JSON.stringify(membrane.coordinates) + " is " + slopeOfMembrane
          );
          return (
            <Graphics
              key={JSON.stringify(membrane.coordinates)}
              interactive
              pointerover={(e: InteractionEvent) => {
                e.currentTarget.scale.set(scale);
              }}
              pointerout={(e: InteractionEvent) => {
                e.currentTarget.scale.set(scale);
              }}
              buttonMode
              scale={scale}
              draw={(g) => {
                g.beginFill(Math.random() * 0xffffff, 1);
                g.drawRoundedRect(
                  membrane.coordinates[
                    Object.is(slopeOfMembrane, 0) ||
                    Object.is(slopeOfMembrane, Infinity)
                      ? 0
                      : 1
                  ].x +
                    offset / 2,
                  membrane.coordinates[
                    Object.is(slopeOfMembrane, 0) ||
                    Object.is(slopeOfMembrane, Infinity)
                      ? 0
                      : 1
                  ].y +
                    offset / 2,
                  Object.is(slopeOfMembrane, 0) ||
                    Object.is(slopeOfMembrane, -0)
                    ? 1.1
                    : offset,
                  Object.is(slopeOfMembrane, Infinity) ||
                    Object.is(slopeOfMembrane, -Infinity)
                    ? 1.1
                    : offset,
                  0
                );

                g.endFill();
              }}
            />
          );
        })}
        {/* <Graphics
          key={JSON.stringify(data.membranes[0].coordinates)}
          interactive
          pointerover={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          pointerout={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          buttonMode
          scale={scale}
          draw={(g) => {
            g.beginFill(Math.random() * 0xffffff, 0.5);
            g.drawRoundedRect(
              data.membranes[0].coordinates[0].x + offset / 2,
              data.membranes[0].coordinates[0].y + offset / 2,
              1.1,
              offset,
              1
            );
            g.endFill();
          }}
        />
        <Graphics
          key={JSON.stringify(data.membranes[1].coordinates)}
          interactive
          pointerover={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          pointerout={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          buttonMode
          scale={scale}
          draw={(g) => {
            g.beginFill(Math.random() * 0xffffff, 0.5);
            g.drawRoundedRect(
              data.membranes[1].coordinates[0].x + offset / 2,
              data.membranes[1].coordinates[0].y + offset / 2,
              offset,
              1.1,
              1
            );
            g.endFill();
          }}
        />
        <Graphics
          key={JSON.stringify(data.membranes[2].coordinates)}
          interactive
          pointerover={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          pointerout={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          buttonMode
          scale={scale}
          draw={(g) => {
            g.beginFill(Math.random() * 0xffffff, 0.5);
            g.drawRoundedRect(
              data.membranes[2].coordinates[1].x + offset / 2,
              data.membranes[2].coordinates[1].y + offset / 2,
              1.1,
              offset,
              1
            );
            g.endFill();
          }}
        />
        <Graphics
          key={JSON.stringify(data.membranes[3].coordinates)}
          interactive
          pointerover={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          pointerout={(e: InteractionEvent) => {
            e.currentTarget.scale.set(scale);
          }}
          buttonMode
          scale={scale}
          draw={(g) => {
            g.beginFill(Math.random() * 0xffffff, 0.5);
            g.drawRoundedRect(
              data.membranes[3].coordinates[1].x + offset / 2,
              data.membranes[3].coordinates[1].y + offset / 2,
              offset,
              1.1,
              1
            );
            g.endFill();
          }}
        /> */}
        <Graphics
          scale={1}
          interactive
          draw={(g) => {
            g.lineStyle(2, 0x0000ff, 1);
            g.beginFill(0xff700b, 1);
            g.drawRect(50, 250, 120, 120);

            g.lineStyle(2, 0xff00ff, 1);
            g.beginFill(0xff00bb, offset * 25);
            g.drawRoundedRect(250, 200, 200, 200, 15);
            g.endFill();

            g.lineStyle(0);
            g.beginFill(0xffff0b, 0.5);
            g.drawCircle(400, 90, 60);
            g.endFill();
          }}
          pointerover={() => {}}
        />
      </Stage>
    </>
  );
};
