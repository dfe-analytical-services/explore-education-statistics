import {
  FullTableQuery,
  TimePeriodQuery,
} from '@common/services/tableBuilderService';

// Encodes a FullTableQuery into a compact query string
export function encodeFullTableQueryToParams(query: FullTableQuery): string {
  const stripDashes = (uuid: string) => uuid.replace(/-/g, '');
  const params: Record<string, string> = {
    sub: stripDashes(query.subjectId),
  };

  if (query.timePeriod) {
    const { startYear, startCode, endYear, endCode } = query.timePeriod;
    params.tp = `${startYear}|${startCode}|${endYear}|${endCode}`;
  }
  if (query.filters && query.filters.length) {
    params.f = query.filters.map(stripDashes).join(',');
  }
  if (query.indicators && query.indicators.length) {
    params.ind = query.indicators.map(stripDashes).join(',');
  }
  if (query.locationIds && query.locationIds.length) {
    params.loc = query.locationIds.map(stripDashes).join(',');
  }

  return new URLSearchParams(params).toString();
}

// Decodes the URL query parameters back into a FullTableQuery
export function decodeParamsToFullTableQuery(
  searchParams: URLSearchParams,
): FullTableQuery {
  const restoreDashes = (hex: string) =>
    hex.replace(/^(.{8})(.{4})(.{4})(.{4})(.{12})$/, '$1-$2-$3-$4-$5');

  const subjectId = restoreDashes(searchParams.get('sub') || '');
  const timePeriodRaw = searchParams.get('tp');
  let timePeriod: TimePeriodQuery | undefined;

  if (timePeriodRaw) {
    const [startYear, startCode, endYear, endCode] = timePeriodRaw.split('|');
    timePeriod = {
      startYear: parseInt(startYear, 10),
      startCode,
      endYear: parseInt(endYear, 10),
      endCode,
    };
  }

  const parseUuids = (paramName: string) => {
    const value = searchParams.get(paramName);
    return value ? value.split(',').map(restoreDashes) : [];
  };

  return {
    subjectId,
    timePeriod,
    filters: parseUuids('f'),
    indicators: parseUuids('ind'),
    locationIds: parseUuids('loc'),
  };
}
