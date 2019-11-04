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
    content: 'My first footnote for subject0 :)',
    subjects: [0],
  },
];
