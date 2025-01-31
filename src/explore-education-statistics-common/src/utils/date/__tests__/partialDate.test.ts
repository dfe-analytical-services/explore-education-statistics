import {
  formatPartialDate,
  parsePartialDateToLocalDate,
} from '@common/utils/date/partialDate';
import { isValid } from 'date-fns';

describe('partialDate', () => {
  describe('parsePartialDateToLocalDate', () => {
    test('returns fully parsed UTC date', () => {
      expect(
        parsePartialDateToLocalDate({
          year: 2020,
          month: 7,
          day: 13,
        }),
      ).toEqual(new Date(2020, 6, 13));
    });

    test('returns parsed UTC date when there is only a year', () => {
      expect(
        parsePartialDateToLocalDate({
          year: 2020,
        }),
      ).toEqual(new Date(2020, 0, 1));
    });

    test('returns parsed UTC date when there is only a year and month', () => {
      expect(
        parsePartialDateToLocalDate({
          year: 2020,
          month: 7,
        }),
      ).toEqual(new Date(2020, 6));
    });

    test('returns invalid UTC date when month is invalid number', () => {
      expect(
        isValid(
          parsePartialDateToLocalDate({
            year: 2020,
            month: 40,
          }),
        ),
      ).toBe(false);
    });

    test('returns invalid UTC date when day is invalid number', () => {
      expect(
        isValid(
          parsePartialDateToLocalDate({
            year: 2020,
            month: 7,
            day: 40,
          }),
        ),
      ).toBe(false);
    });

    test('throw error if missing year', () => {
      expect(() =>
        parsePartialDateToLocalDate({
          year: 0,
          month: 7,
          day: 13,
        }),
      ).toThrowError(/Could not parse invalid PartialDate to date/);
    });
  });

  describe('formatPartialDate', () => {
    test('returns fully formatted date using default format', () => {
      expect(
        formatPartialDate({
          year: 2020,
          month: 7,
          day: 13,
        }),
      ).toBe('13 July 2020');
    });

    test('returns full formatted date using custom format', () => {
      expect(
        formatPartialDate(
          {
            year: 2020,
            month: 7,
            day: 13,
          },
          {
            fullFormat: 'yyyy-MM-dd',
          },
        ),
      ).toBe('2020-07-13');
    });

    test('returns formatted date from only a year and month using default format', () => {
      expect(
        formatPartialDate({
          year: 2020,
          month: 7,
        }),
      ).toBe('July 2020');
    });

    test('returns formatted date from only a year and a month using custom format', () => {
      expect(
        formatPartialDate(
          {
            year: 2020,
            month: 7,
          },
          { monthYearFormat: 'yyyy-MM' },
        ),
      ).toBe('2020-07');
    });

    test('returns formatted date from only a year', () => {
      expect(
        formatPartialDate({
          year: 2020,
        }),
      ).toBe('2020');
    });
  });
});
