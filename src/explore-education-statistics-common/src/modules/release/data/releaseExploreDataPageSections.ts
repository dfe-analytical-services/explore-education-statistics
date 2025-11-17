import { NavItem } from '@common/components/PageNavExpandable';
import { Dictionary } from '@common/types';

export default {
  explore: {
    id: 'explore-section',
    text: 'Explore data used in this release',
    caption:
      'This page provides a range of routes to access data from within this statistical release to suit different users.',
  },
  featuredTables: {
    id: 'featured-tables-section',
    text: 'Featured tables',
    caption:
      "Featured tables are pre-prepared tables created from a statistical release's data sets. They provide statistics that are regularly requested by some users (such as local councils, regional government or government policy teams) and can be adapted to switch between different categories (such as different geographies, time periods or characteristics where available).",
    shortCaption:
      "Featured tables are pre-prepared tables created from a statistical release's data sets. They provide statistics that are regularly requested by users and can be adapted to switch between different categories.",
  },
  datasets: {
    id: 'datasets-section',
    text: 'Data sets: download or create tables',
    caption:
      'Data sets present comprehensive open data from which users can create their own tables using the EES table tool or download a zipped CSV file.',
  },
  supportingFiles: {
    id: 'supporting-files-section',
    text: 'Supporting files',
    caption:
      'Supporting files provide an area for teams to supply non-standard files for download by users where required.',
  },
  dataDashboards: {
    id: 'data-dashboards-section',
    text: 'Data dashboards',
    caption:
      "Data dashboards provide an alternative route to explore a statistical release's data, presenting key statistics and further insights, often via graphical visualisations.",
  },
  dataGuidance: {
    id: 'data-guidance-section',
    text: 'Data guidance',
    caption:
      'Description of the data included in this release, this is a methodology document, providing information on data sources, their coverage and quality and how the data is produced.',
  },
} as const satisfies Dictionary<
  NavItem & { caption: string; shortCaption?: string }
>;
