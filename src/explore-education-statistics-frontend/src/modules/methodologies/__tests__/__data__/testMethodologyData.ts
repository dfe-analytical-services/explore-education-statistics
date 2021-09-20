import { Methodology } from '@common/services/methodologyService';

export const testMethodology: Methodology = {
  id: '0d31edaa-e750-4f39-90c4-9e21774f1ef3',
  title: 'Pupil absence statistics: methodology',
  published: '2021-02-16T15:32:01.2216362',
  slug: 'pupil-absence-in-schools-in-england',
  content: [],
  annexes: [],
  notes: [
    {
      id: '274d624c-2a60-4239-b9a3-a948df66e990',
      displayDate: new Date('2021-09-15T00:00:00'),
      content: 'Latest note',
    },
    {
      id: '30a2457e-910c-438e-806e-781adde29769',
      displayDate: new Date('2021-04-19T00:00:00'),
      content: 'Other note',
    },
    {
      id: 'aa571c68-00d7-48ed-8f53-b783f5b10638',
      displayDate: new Date('2021-03-01T00:00:00'),
      content: 'Earliest note',
    },
  ],
};

export default testMethodology;
