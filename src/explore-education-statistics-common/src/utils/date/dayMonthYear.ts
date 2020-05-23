export interface DayMonthYear {
  day?: number;
  month?: number;
  year?: number;
}

export const dayMonthYearIsComplete = (dmy?: DayMonthYear) => {
  return dmy && dmy.day && dmy.month && dmy.year;
};

export const dayMonthYearIsEmpty = (dmy?: DayMonthYear) => {
  return !dmy || (!dmy.day && !dmy.month && !dmy.year);
};

export const dayMonthYearToDate = (dmy?: DayMonthYear) => {
  if (!dmy) {
    throw Error(`Couldn't convert undefined DayMonthYearValues to Date`);
  }
  if (!dayMonthYearIsComplete(dmy)) {
    throw Error(
      `Couldn't convert DayMonthYearValues ${JSON.stringify(
        dmy,
      )} to Date - missing required value`,
    );
  }
  return new Date(Date.UTC(dmy.year || 0, (dmy.month || 0) - 1, dmy.day));
};
