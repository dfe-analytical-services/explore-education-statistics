import { PermalinkSnapshot } from '@common/services/permalinkSnapshotService';

const testPermalinkSnapshot: PermalinkSnapshot = {
  created: '2020-10-07T12:00:00.00Z',
  dataSetTitle: 'Data Set 1',
  id: 'permalink-1',
  publicationTitle: 'Publication 1',
  status: 'Current',
  table: {
    caption: 'Test table caption 1',
    footnotes: [
      { id: 'footnote-1', label: 'Footnote 1' },
      { id: 'footnote-2', label: 'Footnote 2' },
    ],
    json: {
      thead: [
        [
          {
            colSpan: 1,
            rowSpan: 1,
            tag: 'td',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: '2014/15 Autumn term',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: '2015/16 Autumn term',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: '2016/17 Autumn term',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: '2017/18 Autumn term',
            tag: 'th',
          },
        ],
      ],
      tbody: [
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'State-funded primary',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '420,194',
          },
          {
            tag: 'td',
            text: '385,676',
          },
          {
            tag: 'td',
            text: '403,409',
          },
          {
            tag: 'td',
            text: '402,755',
          },
        ],
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'State-funded primary and secondary',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '567,279',
          },
          {
            tag: 'td',
            text: '516,897',
          },
          {
            tag: 'td',
            text: '543,325',
          },
          {
            tag: 'td',
            text: '540,135',
          },
        ],
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'State-funded secondary',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '147,085',
          },
          {
            tag: 'td',
            text: '131,221',
          },
          {
            tag: 'td',
            text: '139,916',
          },
          {
            tag: 'td',
            text: '137,380',
          },
        ],
      ],
    },
  },
};

export default testPermalinkSnapshot;
