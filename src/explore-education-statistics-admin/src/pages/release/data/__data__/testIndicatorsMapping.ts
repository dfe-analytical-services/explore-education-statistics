import { IndicatorsMapping } from '@admin/services/apiDataSetVersionService';

const testIndicatorsMapping: IndicatorsMapping = {
  candidates: {
    Indicator1: {
      label: 'Indicator 1 label updated',
    },
    Indicator2Updated: {
      label: 'Indicator 2 column name updated',
    },
    Indicator3Updated: {
      label: 'Indicator 3 column name updated',
    },
    Indicator6New: {
      label: 'Indicator 6 new',
    },
  },
  mappings: {
    Indicator1: {
      candidateKey: 'Indicator1',
      publicId: 'indicator-1-public-id',
      source: { label: 'Indicator 1' },
      type: 'AutoMapped',
    },
    Indicator2: {
      candidateKey: 'Indicator2Updated',
      publicId: 'indicator-2-public-id',
      source: { label: 'Indicator 2' },
      type: 'ManualMapped',
    },
    Indicator3: {
      candidateKey: 'Indicator3Updated',
      publicId: 'indicator-3-public-id',
      source: { label: 'Indicator 3' },
      type: 'ManualMapped',
    },
    Indicator4Deleted: {
      publicId: 'indicator-4-public-id',
      source: { label: 'Indicator 4' },
      type: 'AutoNone',
    },
    Indicator5Deleted: {
      publicId: 'indicator-5-public-id',
      source: { label: 'Indicator 5' },
      type: 'ManualNone',
    },
  },
};

export default testIndicatorsMapping;
