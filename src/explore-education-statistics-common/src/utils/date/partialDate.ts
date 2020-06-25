import { format, parse } from 'date-fns';

export interface PartialDate {
  day?: number | string;
  month?: number | string;
  year: number | string;
}

export const isPartialDateEmpty = (dmy?: PartialDate): boolean => {
  return !dmy || (!dmy.day && !dmy.month && !dmy.year);
};

export const isValidPartialDate = (
  dmy?: PartialDate | Partial<PartialDate>,
): dmy is PartialDate => {
  return !!dmy?.year;
};

export const parsePartialDateToUtcDate = (dmy: PartialDate): Date => {
  if (!isValidPartialDate(dmy)) {
    throw new Error(
      `Could not parse invalid PartialDate to date: ${JSON.stringify(dmy)}`,
    );
  }

  if (!dmy.month && !dmy.day) {
    return parse(`${dmy.year}Z`, 'yyyyX', new Date());
  }

  if (!dmy.day) {
    return parse(`${dmy.year}-${dmy.month}Z`, `yyyy-MX`, new Date());
  }

  return parse(
    `${dmy.year}-${dmy.month}-${dmy.day}Z`,
    `yyyy-M-ddX`,
    new Date(),
  );
};

export const formatPartialDate = (
  dmy: PartialDate,
  options: {
    monthYearFormat?: string;
    fullFormat?: string;
  } = {},
): string => {
  const opts = {
    monthYearFormat: 'MMMM yyyy',
    fullFormat: 'dd MMMM yyyy',
    ...options,
  };

  const date = parsePartialDateToUtcDate(dmy);

  try {
    if (!dmy.month && !dmy.day) {
      return format(date, 'yyyy');
    }

    if (!dmy.day) {
      return format(date, opts.monthYearFormat);
    }

    return format(date, opts.fullFormat);
  } catch (err) {
    throw new Error(
      `Could not format invalid date from PartialDate: ${JSON.stringify(dmy)}`,
    );
  }
};
