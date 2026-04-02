export function expectTick(
  container: HTMLElement,
  axis: string,
  child: number,
  value: string,
) {
  expect(
    container.querySelector(
      `.recharts-${axis}Axis-tick-labels .recharts-cartesian-axis-tick-label:nth-child(${child}) .recharts-cartesian-axis-tick-value tspan`,
    ),
  ).toHaveTextContent(value);
}

export function expectTicks(
  container: HTMLElement,
  axis: string,
  ...values: string[]
) {
  values.forEach((value, index) => {
    expectTick(container, axis, index + 1, value);
  });
}
