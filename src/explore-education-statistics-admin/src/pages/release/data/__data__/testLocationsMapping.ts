import { LocationsMapping } from '@admin/services/apiDataSetVersionService';
// localAuthority: autoMapped = 1 & 5, autoNone = 2, manualMapped = 3, manualNone = 4, new = 6 & 7
// region: autoMapped = 8, manualMapped = 9 & 10, new: 11
// englishDevolvedArea: autoMapped = 12 - 20
const testLocationsMapping: LocationsMapping = {
  levels: {
    localAuthority: {
      candidates: {
        Location1Key: {
          label: 'Location 1',
          code: 'location-1-code',
        },
        Location3Key: {
          label: 'Location 3 updated',
          code: 'location-3-code',
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
          type: 'AutoMapped',
          source: {
            label: 'Location 1',
            code: 'location-1-code',
          },
        },
        Location2Key: {
          type: 'AutoNone',
          source: {
            label: 'Location 2',
            code: 'location-2-code',
          },
        },
        Location3Key: {
          candidateKey: 'Location3Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 3',
            code: 'location-3-code',
          },
        },
        Location4Key: {
          type: 'ManualNone',
          source: {
            label: 'Location 4',
            code: 'location-4-code',
          },
        },
        Location5Key: {
          candidateKey: 'Location5Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 5',
            code: 'location-5-code',
          },
        },
      },
    },
    region: {
      candidates: {
        Location8Key: {
          label: 'Location 8',
          code: 'location-8-code',
        },
        Location9Key: {
          label: 'Location 9 updated',
          code: 'location-9-code',
        },
        Location10Key: {
          label: 'Location 10 updated',
          code: 'location-10-code',
        },
        Location11Key: {
          label: 'Location 11',
          code: 'location-11-code',
        },
      },
      mappings: {
        Location8Key: {
          candidateKey: 'Location8Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 8',
            code: 'location-8-code',
          },
        },
        Location9Key: {
          candidateKey: 'Location9Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 9',
            code: 'location-9-code',
          },
        },
        Location10Key: {
          candidateKey: 'Location10Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 10',
            code: 'location-10-code',
          },
        },
      },
    },
    englishDevolvedArea: {
      candidates: {
        Location12Key: {
          label: 'Location 12',
          code: 'location-12-code',
        },
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
        Location16Key: {
          label: 'Location 16',
          code: 'location-16-code',
        },
        Location17Key: {
          label: 'Location 17',
          code: 'location-17-code',
        },
        Location18Key: {
          label: 'Location 18',
          code: 'location-18-code',
        },
        Location19Key: {
          label: 'Location 19',
          code: 'location-19-code',
        },
        Location20Key: {
          label: 'Location 20',
          code: 'location-20-code',
        },
      },
      mappings: {
        Location12Key: {
          candidateKey: 'Location12Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 12',
            code: 'location-12-code',
          },
        },
        Location13Key: {
          candidateKey: 'Location13Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 13',
            code: 'location-13-code',
          },
        },
        Location14Key: {
          candidateKey: 'Location14Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 14',
            code: 'location-14-code',
          },
        },
        Location15Key: {
          candidateKey: 'Location15Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 15',
            code: 'location-15-code',
          },
        },
        Location16Key: {
          candidateKey: 'Location16Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 16',
            code: 'location-16-code',
          },
        },
        Location17Key: {
          candidateKey: 'Location17Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 17',
            code: 'location-17-code',
          },
        },
        Location18Key: {
          candidateKey: 'Location18Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 18',
            code: 'location-18-code',
          },
        },
        Location19Key: {
          candidateKey: 'Location19Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 19',
            code: 'location-19-code',
          },
        },
        Location20Key: {
          candidateKey: 'Location20Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 20',
            code: 'location-20-code',
          },
        },
      },
    },
  },
};

export default testLocationsMapping;
