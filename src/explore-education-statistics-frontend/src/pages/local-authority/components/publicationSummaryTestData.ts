import { PublicationSummaryProps } from './PublicationSummary';

const testData: PublicationSummaryProps[] = [
  {
    keyIndicator: {
      changes: [
        {
          description: '15/16',
          units: 'ppt',
          value: 0.1,
        },
        {
          description: 'national',
          units: 'ppt',
          value: 0.4,
        },
      ],
      link: '#',
      title: 'Sheffield Overall absence',
      units: '%',
      value: 5.1,
    },
    link: '#',
    summary: `
- On average in 2016/17, **pupils missed 8.2 school days**
- Overall and unauthorised absence rates have **increased** since last year
- **One in ten** pupils was persistently absent during the academic year 2016/17`,
    title: 'Pupil absence in schools in England',
  },
  {
    keyIndicator: {
      changes: [
        {
          description: '15/16',
          units: 'ppt',
          value: 0.1,
        },
        {
          description: 'national',
          units: 'ppt',
          value: -4.7,
        },
      ],
      link: '#',
      title: 'Sheffield pupils with at least 2 A-levels',
      units: '%',
      value: 72.7,
    },
    link: '#',
    summary: `
- **Level 3 attainment increased** for students at the end of 16-18 study
- **English and maths average progress increased** for students still working towards 
qualifications below level 3
- Approximately **5% of institutions** fall below the academic and applied **general minimum**`,
    title: 'KS5 A-level results',
  },
];

export default testData;
