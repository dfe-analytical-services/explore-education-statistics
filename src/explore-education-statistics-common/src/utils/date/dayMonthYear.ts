import { format, parse } from 'date-fns';

export interface DayMonthYear {
  day?: number;
  month?: number;
  year: number;
}

export const isDayMonthYearEmpty = (dmy?: DayMonthYear): boolean => {
  return !dmy || (!dmy.day && !dmy.month && !dmy.year);
};

export const isValidDayMonthYear = (
  dmy?: DayMonthYear | Partial<DayMonthYear>,
): dmy is DayMonthYear => {
  return !!dmy?.year;
};

export const parseDayMonthYearToUtcDate = (dmy: DayMonthYear): Date => {
  if (!isValidDayMonthYear(dmy)) {
    throw new Error(
      `Could not parse invalid DayMonthYear to date: ${JSON.stringify(dmy)}`,
    );
  }

  if (!dmy.month && !dmy.day) {
    return parse(`${dmy.year.toString()}Z`, 'yyyyX', new Date());
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

export const formatDayMonthYear = (
  dmy: DayMonthYear,
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

  const date = parseDayMonthYearToUtcDate(dmy);

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
      `Could not format invalid date from DayMonthYear: ${JSON.stringify(dmy)}`,
    );
  }
};
