import { FiltersMapping } from '@admin/services/apiDataSetVersionService';

const testFiltersMapping: FiltersMapping = {
  candidates: {
    Filter1Key: {
      label: 'Filter 1',
      options: {
        Filter1Option1Key: {
          label: 'Filter 1 Option 1',
        },
        Filter1Option2UpdatedKey: {
          label: 'Filter 1 Option 2 updated',
        },
        Filter1Option4UpdatedKey: {
          label: 'Filter 1 Option 4 updated',
        },
      },
    },
    Filter2Key: {
      label: 'Filter 2',
      options: {
        Filter2Option1UpdatedKey: {
          label: 'Filter 2 Option 1 updated',
        },
      },
    },
    Filter3Key: {
      label: 'Filter 3',
      options: {
        Filter3Option1Key: {
          label: 'Filter 3 Option 1',
        },
        Filter3Option2Key: {
          label: 'Filter 3 Option 2',
        },
        Filter3Option3Key: {
          label: 'Filter 3 Option 3',
        },
        Filter3Option4Key: {
          label: 'Filter 3 Option 4',
        },
        Filter3Option5Key: {
          label: 'Filter 3 Option 5',
        },
        Filter3Option6Key: {
          label: 'Filter 3 Option 6',
        },
        Filter3Option7Key: {
          label: 'Filter 3 Option 7',
        },
        Filter3Option8Key: {
          label: 'Filter 3 Option 8',
        },
        Filter3Option9Key: {
          label: 'Filter 3 Option 9',
        },
        Filter3Option10Key: {
          label: 'Filter 3 Option 10',
        },
        Filter3Option11Key: {
          label: 'Filter 3 Option 11',
        },
      },
    },
  },
  mappings: {
    Filter1Key: {
      candidateKey: 'Filter1Key',
      optionMappings: {
        Filter1Option1Key: {
          candidateKey: 'Filter1Option1Key',
          publicId: 'filter-1-option-1-public-id',
          source: { label: 'Filter 1 Option 1' },
          type: 'AutoMapped',
        },
        Filter1Option2Key: {
          publicId: 'filter-1-option-2-public-id',
          source: { label: 'Filter 1 Option 2' },
          type: 'AutoNone',
        },
        Filter1Option3Key: {
          publicId: 'filter-1-option-3-public-id',
          source: { label: 'Filter 1 Option 3' },
          type: 'AutoNone',
        },
        Filter1Option4Key: {
          candidateKey: 'Filter1Option4UpdatedKey',
          publicId: 'filter-1-option-4-public-id',
          source: { label: 'Filter 1 Option 4' },
          type: 'ManualMapped',
        },
      },
      publicId: 'filter-1-public-id',
      source: {
        label: 'Filter 1',
      },
      type: 'AutoMapped',
    },
    Filter2Key: {
      candidateKey: 'Filter2Key',
      optionMappings: {
        Filter2Option1Key: {
          candidateKey: 'Filter2Option1UpdatedKey',
          publicId: 'filter-2-option-1-public-id',
          source: { label: 'Filter 2 Option 1' },
          type: 'ManualMapped',
        },
        Filter2Option2Key: {
          publicId: 'filter-2-option-2-public-id',
          source: { label: 'Filter 2 Option 2' },
          type: 'ManualNone',
        },
        Filter2Option3Key: {
          publicId: 'filter-2-option-3-public-id',
          source: { label: 'Filter 2 Option 3' },
          type: 'AutoNone',
        },
      },
      publicId: 'filter-2-public-id',
      source: {
        label: 'Filter 2',
      },
      type: 'AutoMapped',
    },
    Filter3Key: {
      candidateKey: 'Filter3Key',
      optionMappings: {
        Filter3Option1Key: {
          candidateKey: 'Filter3Option1Key',
          publicId: 'filter-3-option-1-public-id',
          source: { label: 'Filter 3 Option 1' },
          type: 'AutoMapped',
        },
        Filter3Option2Key: {
          candidateKey: 'Filter3Option2Key',
          publicId: 'filter-3-option-2-public-id',
          source: { label: 'Filter 3 Option 2' },
          type: 'AutoMapped',
        },
        Filter3Option3Key: {
          candidateKey: 'Filter3Option3Key',
          publicId: 'filter-3-option-3-public-id',
          source: { label: 'Filter 3 Option 3' },
          type: 'AutoMapped',
        },
        Filter3Option4Key: {
          candidateKey: 'Filter3Option4Key',
          publicId: 'filter-3-option-4-public-id',
          source: { label: 'Filter 3 Option 4' },
          type: 'AutoMapped',
        },
        Filter3Option5Key: {
          candidateKey: 'Filter3Option5Key',
          publicId: 'filter-3-option-5-public-id',
          source: { label: 'Filter 3 Option 5' },
          type: 'AutoMapped',
        },
        Filter3Option6Key: {
          candidateKey: 'Filter3Option6Key',
          publicId: 'filter-3-option-6-public-id',
          source: { label: 'Filter 3 Option 6' },
          type: 'AutoMapped',
        },
        Filter3Option7Key: {
          candidateKey: 'Filter3Option7Key',
          publicId: 'filter-3-option-7-public-id',
          source: { label: 'Filter 3 Option 7' },
          type: 'AutoMapped',
        },
        Filter3Option8Key: {
          candidateKey: 'Filter3Option8Key',
          publicId: 'filter-3-option-8-public-id',
          source: { label: 'Filter 3 Option 8' },
          type: 'AutoMapped',
        },
        Filter3Option9Key: {
          candidateKey: 'Filter3Option9Key',
          publicId: 'filter-3-option-9-public-id',
          source: { label: 'Filter 3 Option 9' },
          type: 'AutoMapped',
        },
        Filter3Option10Key: {
          candidateKey: 'Filter3Option10Key',
          publicId: 'filter-3-option-10-public-id',
          source: { label: 'Filter 3 Option 10' },
          type: 'AutoMapped',
        },
        Filter3Option11Key: {
          candidateKey: 'Filter3Option11Key',
          publicId: 'filter-3-option-11-public-id',
          source: { label: 'Filter 3 Option 11' },
          type: 'AutoMapped',
        },
      },
      publicId: 'filter-3-public-id',
      source: {
        label: 'Filter 3',
      },
      type: 'AutoMapped',
    },
  },
};

export const testFiltersMappingUnmappedColumns: FiltersMapping = {
  candidates: {
    Filter1UpdatedKey: {
      label: 'Filter 1',
      options: {
        Filter1Option1Key: {
          label: 'Filter 1 Option 1',
        },
        Filter1Option2Key: {
          label: 'Filter 1 Option 2',
        },
        Filter1Option3Key: {
          label: 'Filter 1 Option 3',
        },
        Filter1Option4Key: {
          label: 'Filter 1 Option 4',
        },
      },
    },
    Filter2Key: {
      label: 'Filter 2',
      options: {
        Filter2Option1Key: {
          label: 'Filter 2 Option 1',
        },
        Filter2Option2Key: {
          label: 'Filter 2 Option 2',
        },
      },
    },
  },
  mappings: {
    Filter1Key: {
      optionMappings: {
        Filter1Option1Key: {
          publicId: 'filter-1-option-1-public-id',
          source: { label: 'Filter 1 Option 1' },
          type: 'AutoNone',
        },
        Filter1Option2Key: {
          publicId: 'filter-1-option-2-public-id',
          source: { label: 'Filter 1 Option 2' },
          type: 'AutoNone',
        },
        Filter1Option3Key: {
          publicId: 'filter-1-option-3-public-id',
          source: { label: 'Filter 1 Option 3' },
          type: 'AutoNone',
        },
        Filter1Option4Key: {
          publicId: 'filter-1-option-4-public-id',
          source: { label: 'Filter 1 Option 4' },
          type: 'AutoNone',
        },
      },
      publicId: 'filter-1-public-id',
      source: {
        label: 'Filter 1',
      },
      type: 'AutoNone',
    },
    Filter2Key: {
      candidateKey: 'Filter2Key',
      optionMappings: {
        Filter2Option1Key: {
          candidateKey: 'Filter2Option1Key',
          publicId: 'filter-2-option-1-public-id',
          source: { label: 'Filter 2 Option 1' },
          type: 'AutoMapped',
        },
        Filter2Option2Key: {
          candidateKey: 'Filter2Option2Key',
          publicId: 'filter-2-option-2-public-id',
          source: { label: 'Filter 2 Option 2' },
          type: 'AutoMapped',
        },
      },
      publicId: 'filter-2-public-id',
      source: {
        label: 'Filter 2',
      },
      type: 'AutoMapped',
    },
  },
};

export default testFiltersMapping;
