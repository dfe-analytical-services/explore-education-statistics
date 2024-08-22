import { LocationsMapping } from '@admin/services/apiDataSetVersionService';

const testLocationsMapping: LocationsMapping = {
  levels: {
    // autoMapped = 1 & 5
    // autoNone = 2
    // manualMapped = 3
    // manualNone = 4
    // new = 6 & 7
    localAuthority: {
      candidates: {
        Location1Key: {
          label: 'Location 1',
          code: 'location-1-code',
        },
        Location3UpdatedKey: {
          label: 'Location 3 updated',
          code: 'location-3-code-updated',
        },
        Location5Key: {
          label: 'Location 5',
          code: 'location-5-code',
        },
        Location6Key: {
          label: 'Location 6',
          code: 'location-6-code',
        },
        Location7Key: {
          label: 'Location 7',
          code: 'location-7-code',
        },
      },
      mappings: {
        Location1Key: {
          candidateKey: 'Location1Key',
          publicId: 'location-1-public-id',
          source: {
            label: 'Location 1',
            code: 'location-1-code',
          },
          type: 'AutoMapped',
        },
        Location2Key: {
          publicId: 'location-2-public-id',
          source: {
            label: 'Location 2',
            code: 'location-2-code',
          },
          type: 'AutoNone',
        },
        Location3Key: {
          candidateKey: 'Location3UpdatedKey',
          publicId: 'location-3-public-id',
          source: {
            label: 'Location 3',
            code: 'location-3-code',
          },
          type: 'ManualMapped',
        },
        Location4Key: {
          publicId: 'location-4-public-id',
          source: {
            label: 'Location 4',
            code: 'location-4-code',
          },
          type: 'ManualNone',
        },
        Location5Key: {
          candidateKey: 'Location5Key',
          publicId: 'location-5-public-id',
          source: {
            label: 'Location 5',
            code: 'location-5-code',
          },
          type: 'AutoMapped',
        },
      },
    },
    // autoMapped = 8
    // manualMapped = 9 & 10
    // new: 11
    region: {
      candidates: {
        Location8Key: {
          label: 'Location 8',
          code: 'location-8-code',
        },
        Location9UpdatedKey: {
          label: 'Location 9 updated',
          code: 'location-9-code',
        },
        Location10UpdatedKey: {
          label: 'Location 10 updated',
          code: 'location-10-code',
        },
        Location11Key: {
          label: 'Location 11',
          code: 'location-11-code',
        },
        Location12Key: {
          label: 'Location 12',
          code: 'location-12-code',
        },
      },
      mappings: {
        Location8Key: {
          candidateKey: 'Location8Key',
          publicId: 'location-8-public-id',
          source: {
            label: 'Location 8',
            code: 'location-8-code',
          },
          type: 'AutoMapped',
        },
        Location9Key: {
          candidateKey: 'Location9UpdatedKey',
          publicId: 'location-9-public-id',
          source: {
            label: 'Location 9',
            code: 'location-9-code',
          },
          type: 'ManualMapped',
        },
        Location10Key: {
          candidateKey: 'Location10UpdatedKey',
          publicId: 'location-10-public-id',
          source: {
            label: 'Location 10',
            code: 'location-10-code',
          },
          type: 'ManualMapped',
        },
        Location12Key: {
          publicId: 'location-12-public-id',
          source: {
            label: 'Location 12',
            code: 'location-12-code',
          },
          type: 'ManualNone',
        },
      },
    },
    // autoMapped = 13 - 15
    englishDevolvedArea: {
      candidates: {
        Location13Key: {
          label: 'Location 13',
          code: 'location-13-code',
        },
        Location14Key: {
          label: 'Location 14',
          code: 'location-14-code',
        },
        Location15Key: {
          label: 'Location 15',
          code: 'location-15-code',
        },
      },
      mappings: {
        Location13Key: {
          candidateKey: 'Location13Key',
          publicId: 'location-13-public-id',
          source: {
            label: 'Location 13',
            code: 'location-13-code',
          },
          type: 'AutoMapped',
        },
        Location14Key: {
          candidateKey: 'Location14Key',
          publicId: 'location-14-public-id',
          source: {
            label: 'Location 14',
            code: 'location-14-code',
          },
          type: 'AutoMapped',
        },
        Location15Key: {
          candidateKey: 'Location15Key',
          publicId: 'location-15-public-id',
          source: {
            label: 'Location 15',
            code: 'location-15-code',
          },
          type: 'AutoMapped',
        },
      },
    },
  },
};

export default testLocationsMapping;

export const testLocationsMappingGroups: LocationsMapping = {
  levels: {
    ...testLocationsMapping.levels,
    // Group has been deleted
    sponsor: {
      candidates: {},
      mappings: {
        Location16Key: {
          publicId: 'location-16-public-id',
          source: {
            label: 'Location 16',
            code: 'location-16-code',
          },
          type: 'AutoNone',
        },
        Location17Key: {
          publicId: 'location-17-public-id',
          source: {
            label: 'Location 17',
            code: 'location-17-code',
          },
          type: 'AutoNone',
        },
      },
    },
    // Group has been deleted
    localAuthorityDistrict: {
      candidates: {},
      mappings: {
        Location18Key: {
          publicId: 'location-18-public-id',
          source: {
            label: 'Location 18',
            code: 'location-18-code',
          },
          type: 'AutoNone',
        },
        Location19Key: {
          publicId: 'location-19-public-id',
          source: {
            label: 'Location 19',
            code: 'location-19-code',
          },
          type: 'AutoNone',
        },
      },
    },
    // Group has been added
    ward: {
      candidates: {
        Location20Key: {
          label: 'Location 20',
          code: 'location-20-code',
        },
        Location21Key: {
          label: 'Location 21',
          code: 'location-21-code',
        },
      },
      mappings: {},
    },
    // Group has been added
    opportunityArea: {
      candidates: {
        Location22Key: {
          label: 'Location 22',
          code: 'location-22-code',
        },
        Location23Key: {
          label: 'Location 23',
          code: 'location-23-code',
        },
      },
      mappings: {},
    },
  },
};
