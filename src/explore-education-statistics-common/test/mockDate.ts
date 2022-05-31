/* eslint-disable @typescript-eslint/ban-ts-comment */
import { isValid } from 'date-fns';

const RealDate = Date;

let now: Date | null = null;

class MockedDate extends RealDate {
  constructor(...dateArgs: never[]) {
    super();

    switch (dateArgs.length) {
      case 0:
        return now !== null ? now : new RealDate();
      default:
        // @ts-ignore
        return new RealDate(...dateArgs);
    }
  }

  static UTC = RealDate.UTC;

  static now() {
    return new MockedDate().valueOf();
  }

  static parse = RealDate.parse;

  static toString = RealDate.toString;
}

const mockDate = {
  set(date: string | number | Date): void {
    const dateObj = new Date(date);

    if (!isValid(dateObj)) {
      throw new Error(`Could not set invalid date: ${dateObj}`);
    }

    // @ts-ignore
    // eslint-disable-next-line no-global-assign
    Date = MockedDate;

    now = dateObj;
  },
  reset(): void {
    // @ts-ignore
    // eslint-disable-next-line no-global-assign
    Date = RealDate;
  },
};

export default mockDate;
