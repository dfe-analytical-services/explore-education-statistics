import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  GeoJsonFeature,
  TableDataResponse,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import produce from 'immer';

export const testMapConfiguration: Chart = {
  axes: {
    major: {
      type: 'major',
      groupBy: 'locations',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-rate',
          filters: ['school-type-total', 'characteristic-total'],
          timePeriod: '2016_AY',
        },
        {
          indicator: 'overall-absence-rate',
          filters: ['school-type-total', 'characteristic-total'],
          timePeriod: '2016_AY',
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      showGrid: true,
      min: 0,
      size: 50,
      tickConfig: 'default',
      tickSpacing: 1,
    },
  },
  legend: {
    position: 'none',
    items: [
      {
        dataSet: {
          indicator: 'authorised-absence-rate',
          filters: ['school-type-total', 'characteristic-total'],
          timePeriod: '2016_AY',
        },
        label: 'Authorised absence rate (2016/17)',
        colour: '#4763a5',
        symbol: 'circle',
        lineStyle: 'solid',
      },
      {
        dataSet: {
          indicator: 'overall-absence-rate',
          filters: ['school-type-total', 'characteristic-total'],
          timePeriod: '2016_AY',
        },
        label: 'Overall absence rate (2016/17)',
        colour: '#f5a450',
        symbol: 'square',
        lineStyle: 'solid',
      },
    ],
  },
  type: 'map',
  title: '',
  alt: '',
  height: 600,
};

export const testMapTableData: TableDataResponse = {
  subjectMeta: {
    filters: {
      SchoolType: {
        totalValue: 'school-type-total',
        hint: 'Filter by school type',
        legend: 'School type',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'Total',
                value: 'school-type-total',
              },
            ],
            order: 0,
          },
        },
        order: 0,
        name: 'school_type',
      },
      Characteristic: {
        totalValue: 'characteristic-total',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        options: {
          Total: {
            id: 'total',
            label: 'Total',
            options: [
              {
                label: 'Total',
                value: 'characteristic-total',
              },
            ],
            order: 0,
          },
        },
        order: 1,
        name: 'characteristic',
      },
    },
    footnotes: [],
    indicators: [
      {
        label: 'Authorised absence rate',
        unit: '%',
        value: 'authorised-absence-rate',
        name: 'sess_authorised_percent',
      },
      {
        label: 'Unauthorised absence rate',
        unit: '%',
        value: 'unauthorised-absence-rate',
        name: 'sess_unauthorised_percent',
      },
      {
        label: 'Overall absence rate',
        unit: '%',
        value: 'overall-absence-rate',
        name: 'sess_overall_percent',
      },
    ],
    locations: {
      localAuthorityDistrict: [
        {
          geoJson: [
            {
              type: 'Feature',
              properties: {
                code: 'E08000035',
                lat: 53.8227005,
                long: -1.50735998,
                name: 'Leeds',
              },
              geometry: {
                type: 'Polygon',
                coordinates: [
                  [
                    [-1.7272122459449797, 53.91018207872648],
                    [-1.717954443478028, 53.90852764296584],
                    [-1.7070789195040712, 53.91911616928902],
                    [-1.6846228682918523, 53.91057304972962],
                    [-1.6550783672094413, 53.91246927515217],
                    [-1.6516686654393964, 53.905648429113306],
                    [-1.6205320493000477, 53.90337996796216],
                    [-1.5999880820744297, 53.909761262846175],
                    [-1.5874815693621365, 53.901232639071424],
                    [-1.5836196478933626, 53.90927867713697],
                    [-1.5518381392479723, 53.90292494512212],
                    [-1.5487873285399447, 53.91089840622154],
                    [-1.4987749122035887, 53.91529113779204],
                    [-1.4627085229863468, 53.905999070515456],
                    [-1.4329915005984508, 53.91078879577366],
                    [-1.4325952716325585, 53.927220384612674],
                    [-1.406703657755602, 53.92783329810471],
                    [-1.3971764508591014, 53.94251885122632],
                    [-1.3403994347078465, 53.94587147012703],
                    [-1.3447061182908924, 53.93948488503583],
                    [-1.3069564857191547, 53.93440698081743],
                    [-1.3089868144308223, 53.924080718893116],
                    [-1.294174854693236, 53.92704695461069],
                    [-1.3073045344479266, 53.92147712835671],
                    [-1.297198396373909, 53.921664723786655],
                    [-1.3005829268309885, 53.90746820663706],
                    [-1.3137177827199378, 53.90468480675319],
                    [-1.32226598971721, 53.90043984020674],
                    [-1.3125983886506065, 53.86529922958467],
                    [-1.3529652216845465, 53.85673325169743],
                    [-1.3265529959038338, 53.84040593233112],
                    [-1.3360926614813968, 53.83357240333493],
                    [-1.3036195737971894, 53.8165185676977],
                    [-1.3148682830964724, 53.80954017119282],
                    [-1.3138067695442166, 53.78153872204979],
                    [-1.2903822834119523, 53.763177059399666],
                    [-1.294938245772257, 53.75544536456313],
                    [-1.3122350871856157, 53.755871188435144],
                    [-1.3153531165186367, 53.743663244787044],
                    [-1.3020395795319009, 53.74170704480217],
                    [-1.399718577309876, 53.71929166929782],
                    [-1.443146384139212, 53.72820985550881],
                    [-1.488295114230212, 53.72774054010061],
                    [-1.4957249779495894, 53.72229197259589],
                    [-1.5103922671999332, 53.72967359590035],
                    [-1.559319574765559, 53.69896752465426],
                    [-1.5711189325729777, 53.70638866805424],
                    [-1.5922009694165207, 53.71852079212872],
                    [-1.6233681480036957, 53.718531895740604],
                    [-1.637878343763955, 53.74771335655538],
                    [-1.6584406304426313, 53.74544219729005],
                    [-1.6816173762005604, 53.75645342846266],
                    [-1.6495600746681067, 53.76818241321476],
                    [-1.6404018344611493, 53.779668130396786],
                    [-1.674229516312656, 53.77998945828715],
                    [-1.6822666232411927, 53.78639965891872],
                    [-1.7119921564908207, 53.78305472262842],
                    [-1.6950877674837486, 53.85752312689574],
                    [-1.7152825916319183, 53.8662298678809],
                    [-1.7605081860697267, 53.863594391163105],
                    [-1.8004213715201693, 53.885950004226295],
                    [-1.7874617019087486, 53.896885763481976],
                    [-1.7563516693734762, 53.884691318751514],
                    [-1.7254273998291672, 53.885674482040415],
                    [-1.7170536689590863, 53.89224674755545],
                    [-1.7320451637871652, 53.89592831881758],
                    [-1.7272122459449797, 53.91018207872648],
                  ],
                ],
              },
            },
          ],
          id: 'leeds-id',
          label: 'Leeds',
          value: 'leeds',
        },
        {
          geoJson: [
            {
              type: 'Feature',
              properties: {
                code: 'E08000003',
                lat: 53.4701004,
                long: -2.23358989,
                name: 'Manchester',
              },
              geometry: {
                type: 'Polygon',
                coordinates: [
                  [
                    [-2.256968309132829, 53.517929741444135],
                    [-2.2466472820885373, 53.52922026098607],
                    [-2.264116667350858, 53.52467436850162],
                    [-2.2682728781493884, 53.53740977805289],
                    [-2.2517394116511085, 53.544592344156214],
                    [-2.237231991304388, 53.5388443470993],
                    [-2.2187272688353845, 53.54389067881605],
                    [-2.1860163351638926, 53.529043344181765],
                    [-2.1547626759573943, 53.51801698269652],
                    [-2.1787121453620224, 53.50583388352356],
                    [-2.163031628671863, 53.49283305483665],
                    [-2.1680650429797934, 53.48007144614911],
                    [-2.1468288166179836, 53.46764243105517],
                    [-2.1584527669095563, 53.454920504584805],
                    [-2.172881314905998, 53.447984724400776],
                    [-2.1764722663591245, 53.43308285192535],
                    [-2.188541239225052, 53.436636390151286],
                    [-2.2134291350110566, 53.41889876987021],
                    [-2.212596094950082, 53.406951775470354],
                    [-2.246827686265271, 53.39604378917838],
                    [-2.240791191449222, 53.35955788381937],
                    [-2.256448409259317, 53.36066053681609],
                    [-2.3013601575183427, 53.34010440227832],
                    [-2.296931389234865, 53.348567573708],
                    [-2.3139994536166966, 53.35740831270317],
                    [-2.2985957824914305, 53.36080866990268],
                    [-2.2856655049714845, 53.376228889892005],
                    [-2.3199185005518843, 53.411607389277734],
                    [-2.2977148662573774, 53.408345421876234],
                    [-2.2777787564846306, 53.415491720018856],
                    [-2.273620032715012, 53.422484607967114],
                    [-2.3001855197190033, 53.43639968191176],
                    [-2.2536882488279635, 53.45992897245486],
                    [-2.2653212275754493, 53.472712202175614],
                    [-2.245139805911553, 53.48607090644284],
                    [-2.2554252226226743, 53.49910321037437],
                    [-2.245230086297781, 53.512140763983076],
                    [-2.256968309132829, 53.517929741444135],
                  ],
                ],
              },
            },
          ],
          id: 'manchester-id',
          label: 'Manchester',
          value: 'manchester',
        },
        {
          geoJson: [
            {
              type: 'Feature',
              properties: {
                code: 'E08000019',
                lat: 53.40359879,
                long: -1.54253995,
                name: 'Sheffield',
              },
              geometry: {
                type: 'Polygon',
                coordinates: [
                  [
                    [-1.8014715033912057, 53.48097560991305],
                    [-1.7836488291511283, 53.48478285272588],
                    [-1.7387080538018382, 53.477135662039636],
                    [-1.7319829811985903, 53.49088024254383],
                    [-1.7010885669831688, 53.503104212647564],
                    [-1.606589808262866, 53.492890405607184],
                    [-1.5496575916817696, 53.47847698798956],
                    [-1.5390462874158648, 53.46169804224438],
                    [-1.5512605935136123, 53.44618716807899],
                    [-1.5329258046160654, 53.43829776113654],
                    [-1.5131705099706583, 53.45140518277948],
                    [-1.5148138976029188, 53.46364889651925],
                    [-1.4947491871088254, 53.48625105291211],
                    [-1.455218472884002, 53.471732632276556],
                    [-1.441534968325627, 53.44541567427531],
                    [-1.408503943832695, 53.42033120495176],
                    [-1.3804382118653107, 53.42415031965291],
                    [-1.3935294144317814, 53.41998475882009],
                    [-1.3757982269794662, 53.393656289530405],
                    [-1.391398638792512, 53.38330270424309],
                    [-1.3319764367518476, 53.352336949476495],
                    [-1.3246686033954158, 53.328790944960055],
                    [-1.3377505845683773, 53.31582866277357],
                    [-1.3867399973409786, 53.317613155873936],
                    [-1.3865720219173672, 53.33488340404759],
                    [-1.4110347862499268, 53.34197378875228],
                    [-1.4208680587913165, 53.33454949906736],
                    [-1.441973628194892, 53.33747429132319],
                    [-1.4598813784770373, 53.330671244521334],
                    [-1.4552247535400316, 53.32185096624322],
                    [-1.4679137905783164, 53.3171019223235],
                    [-1.5021329399846552, 53.31756415774674],
                    [-1.5367699247001578, 53.304730929996126],
                    [-1.5617309875466472, 53.30639373334149],
                    [-1.5617321345916204, 53.31595334096692],
                    [-1.5805350200762223, 53.31172064079885],
                    [-1.584903138258602, 53.32157056254325],
                    [-1.5990944347574434, 53.311300865010196],
                    [-1.6092027564410094, 53.32264671252802],
                    [-1.6254949460034014, 53.31640681674268],
                    [-1.6327282107295802, 53.3208220813097],
                    [-1.6123042774987584, 53.34320139833299],
                    [-1.590549124881347, 53.345903616814866],
                    [-1.6635498226489183, 53.3665780936865],
                    [-1.6538184927068351, 53.39189377410612],
                    [-1.7049660229668144, 53.40503925119144],
                    [-1.7085327549922174, 53.41732278959336],
                    [-1.7397624272907029, 53.4209714503034],
                    [-1.7453232434846755, 53.462158921862724],
                    [-1.768437875220393, 53.46475052985636],
                    [-1.8014715033912057, 53.48097560991305],
                  ],
                ],
              },
            },
          ],
          id: 'sheffield-id',
          label: 'Sheffield',
          value: 'sheffield',
        },
      ],
    },
    boundaryLevels: [
      {
        id: 8,
        label: 'Local Authority Districts (April 2019) Boundaries UK BUC',
      },
      {
        id: 3,
        label:
          'Local Authority Districts December 2017 Ultra Generalised Clipped Boundaries in United Kingdom WGS84',
      },
    ],
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    timePeriodRange: [{ code: 'AY', label: '2016/17', year: 2016 }],
    geoJsonAvailable: true,
  },
  results: [
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'localAuthorityDistrict',
      locationId: 'leeds-id',
      measures: {
        'authorised-absence-rate': '3.5',
        'overall-absence-rate': '4.8',
        'unauthorised-absence-rate': '1.7',
      },
      timePeriod: '2016_AY',
    },
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'localAuthorityDistrict',
      locationId: 'manchester-id',
      measures: {
        'authorised-absence-rate': '3',
        'overall-absence-rate': '4.7',
        'unauthorised-absence-rate': '1.6',
      },
      timePeriod: '2016_AY',
    },
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'localAuthorityDistrict',
      locationId: 'sheffield-id',
      measures: {
        'authorised-absence-rate': '4',
        'overall-absence-rate': '5.1',
        'unauthorised-absence-rate': '2',
      },
      timePeriod: '2016_AY',
    },
  ],
};

export const testMapTableDataRegion = produce(testMapTableData, draft => {
  draft.subjectMeta.locations = {
    region: [
      {
        geoJson: [
          {
            type: 'Feature',
            properties: {
              code: 'region-1-code',
              lat: 53.8227005,
              long: -1.50735998,
              name: 'Region 1',
            },
            geometry: {
              type: 'Polygon',
              coordinates: [],
            },
          },
        ],
        id: 'region-1-id',
        label: 'Region 1',
        value: 'region-1-code',
      },
      {
        geoJson: [
          {
            type: 'Feature',
            properties: {
              code: 'region-2-code',
              lat: 53.4701004,
              long: -2.23358989,
              name: 'Region 2',
            },
            geometry: {
              type: 'Polygon',
              coordinates: [],
            },
          },
        ],
        id: 'region-2-id',
        label: 'Region 2',
        value: 'region-2-code',
      },
      {
        geoJson: [
          {
            type: 'Feature',
            properties: {
              code: 'region-3-code',
              lat: 53.40359879,
              long: -1.54253995,
              name: 'Region 3',
            },
            geometry: {
              type: 'Polygon',
              coordinates: [],
            },
          },
        ],
        id: 'region-3-id',
        label: 'Region 3',
        value: 'region-3-code',
      },
    ],
  };
  draft.results = [
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'region',
      locationId: 'region-1-id',
      measures: {
        'authorised-absence-rate': '3.5',
        'overall-absence-rate': '4.8',
        'unauthorised-absence-rate': '1.7',
      },
      timePeriod: '2016_AY',
    },
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'region',
      locationId: 'region-2-id',
      measures: {
        'authorised-absence-rate': '3',
        'overall-absence-rate': '4.7',
        'unauthorised-absence-rate': '1.6',
      },
      timePeriod: '2016_AY',
    },
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'region',
      locationId: 'region-3-id',
      measures: {
        'authorised-absence-rate': '4',
        'overall-absence-rate': '5.1',
        'unauthorised-absence-rate': '2',
      },
      timePeriod: '2016_AY',
    },
  ];
});

export const testMapTableDataMixed = produce(testMapTableData, draft => {
  draft.subjectMeta.locations = {
    localAuthority: [
      {
        geoJson: [
          {
            type: 'Feature',
            properties: {
              code: 'la-1-code',
              lat: 53.8227005,
              long: -1.50735998,
              name: 'LA 1',
            },
            geometry: {
              type: 'Polygon',
              coordinates: [],
            },
          },
        ],
        id: 'la-1-id',
        label: 'LA 1',
        value: 'la-1-code',
      },
    ],
    country: [
      {
        geoJson: [
          {
            type: 'Feature',
            properties: {
              code: 'country-1-code',
              lat: 53.4701004,
              long: -2.23358989,
              name: 'Country 1',
            },
            geometry: {
              type: 'Polygon',
              coordinates: [],
            },
          },
        ],
        id: 'country-1-id',
        label: 'Country 1',
        value: 'country-1-code',
      },
    ],
    region: [
      {
        geoJson: [
          {
            type: 'Feature',
            properties: {
              code: 'region-3-code',
              lat: 53.40359879,
              long: -1.54253995,
              name: 'Region 3',
            },
            geometry: {
              type: 'Polygon',
              coordinates: [],
            },
          },
        ],
        id: 'region-3-id',
        label: 'Region 3',
        value: 'region-3-code',
      },
    ],
  };
  draft.results = [
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1-id',
      measures: {
        'authorised-absence-rate': '3.5',
        'overall-absence-rate': '4.8',
        'unauthorised-absence-rate': '1.7',
      },
      timePeriod: '2016_AY',
    },
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'country',
      locationId: 'country-1-id',
      measures: {
        'authorised-absence-rate': '3',
        'overall-absence-rate': '4.7',
        'unauthorised-absence-rate': '1.6',
      },
      timePeriod: '2016_AY',
    },
    {
      filters: ['characteristic-total', 'school-type-total'],
      geographicLevel: 'region',
      locationId: 'region-3-id',
      measures: {
        'authorised-absence-rate': '4',
        'overall-absence-rate': '5.1',
        'unauthorised-absence-rate': '2',
      },
      timePeriod: '2016_AY',
    },
  ];
});

const testGeoJsonFeature: GeoJsonFeature = {
  type: 'Feature',
  geometry: { type: 'Polygon', coordinates: [] },
  properties: {
    code: '',
    name: '',
    lat: 1,
    long: 2,
  },
};

export const testsMixedLocationsFullTableMeta: FullTableMeta = {
  geoJsonAvailable: false,
  publicationName: 'Pupil absence in schools in England',
  subjectName: 'Absence by characteristic',
  footnotes: [],
  boundaryLevels: [],
  filters: {
    Characteristic: {
      name: 'Characteristic total',
      options: [
        new CategoryFilter({
          value: 'characteristic-total',
          label: 'Total',
          group: 'Gender',
          category: 'Characteristic',
        }),
      ],
      order: 0,
    },
  },
  indicators: [
    new Indicator({
      label: 'Authorised absence rate',
      value: 'authorised-absence-rate',
      unit: '%',
      name: 'sess_authorised_percent',
    }),
  ],
  locations: [
    new LocationFilter({
      id: 'england-id',
      value: 'england',
      label: 'England',
      group: 'England',
      level: 'country',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'darlington-id',
      value: 'darlington',
      label: 'Darlington',
      level: 'localAuthority',
      group: 'North East',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'newcastle-id',
      value: 'newcastle',
      label: 'Newcastle upon Tyne',
      group: 'North East',
      level: 'localAuthority',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'rotherham-id',
      value: 'rotherham',
      label: 'Rotherham',
      group: 'Yorkshire and the Humber',
      level: 'localAuthority',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'sheffield-id',
      value: 'sheffield',
      label: 'Sheffield',
      group: 'Yorkshire and the Humber',
      level: 'localAuthority',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'north-east-id',
      value: 'north-east',
      label: 'North East',
      group: 'North East',
      level: 'region',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'north-west-id',
      value: 'north-west',
      label: 'North West',
      group: 'North West',
      level: 'region',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'sheffield-lad-id',
      value: 'sheffield-lad',
      label: 'Sheffield LAD',
      group: 'Yorkshire and the Humber',
      level: 'localAuthorityDistrict',
      geoJson: [testGeoJsonFeature],
    }),
    new LocationFilter({
      id: 'rotherham-lad-id',
      value: 'rotherham-lad',
      label: 'Rotherham LAD',
      group: 'Yorkshire and the Humber',
      level: 'localAuthorityDistrict',
      geoJson: [testGeoJsonFeature],
    }),
  ],
  timePeriodRange: [
    new TimePeriodFilter({
      code: 'AY',
      year: 2016,
      label: '2016/17',
      order: 0,
    }),
  ],
};

export const testsMixedLocationsTableData: TableDataResult[] = [
  {
    filters: ['characteristic-total'],
    geographicLevel: 'country',
    locationId: 'england-id',
    measures: {
      'authorised-absence-rate': '3.5',
    },
    timePeriod: '2016_AY',
  },
  {
    filters: ['characteristic-total'],
    geographicLevel: 'region',
    locationId: 'north-east-id',
    measures: {
      'authorised-absence-rate': '3',
    },
    timePeriod: '2016_AY',
  },
  {
    filters: ['characteristic-total'],
    geographicLevel: 'region',
    locationId: 'north-west-id',
    measures: {
      'authorised-absence-rate': '4',
    },
    timePeriod: '2016_AY',
  },
  {
    filters: ['characteristic-total'],
    geographicLevel: 'localAuthority',
    locationId: 'rotherham-id',
    measures: {
      'authorised-absence-rate': '4',
    },
    timePeriod: '2016_AY',
  },
  {
    filters: ['characteristic-total'],
    geographicLevel: 'localAuthority',
    locationId: 'sheffield-id',
    measures: {
      'authorised-absence-rate': '4',
    },
    timePeriod: '2016_AY',
  },
  {
    filters: ['characteristic-total'],
    geographicLevel: 'localAuthority',
    locationId: 'newcastle-id',
    measures: {
      'authorised-absence-rate': '4',
    },
    timePeriod: '2016_AY',
  },
  {
    filters: ['characteristic-total'],
    geographicLevel: 'localAuthority',
    locationId: 'darlington-id',
    measures: {
      'authorised-absence-rate': '4',
    },
    timePeriod: '2016_AY',
  },
];

export const testsMixedLocationsTableDataWithLADs: TableDataResult[] = [
  {
    filters: ['characteristic-total'],
    geographicLevel: 'localAuthorityDistrict',
    locationId: 'sheffield-lad-id',
    measures: {
      'authorised-absence-rate': '4',
    },
    timePeriod: '2016_AY',
  },
  {
    filters: ['characteristic-total'],
    geographicLevel: 'localAuthorityDistrict',
    locationId: 'rotherham-lad-id',
    measures: {
      'authorised-absence-rate': '4',
    },
    timePeriod: '2016_AY',
  },
];
