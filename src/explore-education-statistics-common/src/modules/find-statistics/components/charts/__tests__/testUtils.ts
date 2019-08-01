export function expectTick(
  container: HTMLElement,
  axis: string,
  child: number,
  value: string,
) {
  expect(
    container.querySelector(
      `.${axis}Axis .recharts-cartesian-axis-tick:nth-child(${child}) .recharts-cartesian-axis-tick-value tspan`,
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
