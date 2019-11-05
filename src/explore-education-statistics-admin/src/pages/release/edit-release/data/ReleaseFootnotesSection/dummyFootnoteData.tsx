import { FootnoteMeta, Footnote } from '.';

export const dummyFootnoteMeta: FootnoteMeta = {
  0: {
    subjectName: 'subject0',
    subjectId: 0,
    indicators: {
      0: {
        label: 'indicatorLabel',
        options: [
          {
            label: 'indicatorItem',
            value: '0',
            unit: '%',
          },
        ],
      },
    },
    filters: {},
  },
};

export const dummyFootnotes: Footnote[] = [
  {
    id: '0',
    content: 'My first footnote for subject0 :)',
    subjects: [0],
  },
  {
    id: '1',
    content: 'A Footnote',
    indicators: [0, 1],
    filters: [2, 3],
  },
];
