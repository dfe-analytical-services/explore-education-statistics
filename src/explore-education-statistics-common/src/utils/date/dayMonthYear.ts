import { parse } from 'date-fns';

export interface DayMonthYear {
  day: number | null;
  month: number | null;
  year: number | null;
}

export const dayMonthYearIsComplete = (dmy?: DayMonthYear) => {
  return dmy && dmy.day && dmy.month && dmy.year;
};

export const dayMonthYearIsEmpty = (dmy?: DayMonthYear) => {
  return !dmy || (!dmy.day && !dmy.month && !dmy.year);
};

export const dayMonthYearToDate = (dmy?: DayMonthYear) => {
  if (!dmy || !dayMonthYearIsComplete(dmy)) {
    throw Error(
      `Could not convert invalid DayMonthYear to date: ${JSON.stringify(dmy)}`,
    );
  }

  return parse(
    `${dmy.year}-${dmy.month}-${dmy.day}`,
    'yyyy-M-d',
    new Date(Date.UTC(dmy.year || 0, (dmy.month || 0) - 1, dmy.day || 0)),
  );
};
