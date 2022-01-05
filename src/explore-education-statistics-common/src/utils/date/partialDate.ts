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

export const parsePartialDateToLocalDate = (dmy: PartialDate): Date => {
  if (!isValidPartialDate(dmy)) {
    throw new Error(
      `Could not parse invalid PartialDate to date: ${JSON.stringify(dmy)}`,
    );
  }

  return parse(
    `${dmy.year}-${dmy.month || 1}-${dmy.day || 1}`,
    'yyyy-M-dd',
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
    fullFormat: 'd MMMM yyyy',
    ...options,
  };

  const date = parsePartialDateToLocalDate(dmy);

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
