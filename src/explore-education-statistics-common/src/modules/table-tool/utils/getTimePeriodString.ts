export default function getTimePeriodString(timePeriods: {
  from?: string;
  to?: string;
}): string | undefined {
  const { from, to } = timePeriods;

  if (from && to) {
    return from === to ? from : `${from} to ${to}`;
  }

  return from || to;
}
