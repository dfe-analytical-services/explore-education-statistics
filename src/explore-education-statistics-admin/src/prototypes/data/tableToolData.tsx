import {
  Subject,
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { PublicationReleaseSummary } from '@common/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const subjects: Subject[] = [
  {
    id: '5e989b4d-b23e-4582-9984-08d987eb588f',
    name:
      'A1 National time series of children in need, referrals and assessments, England 2013 to 2021',
    content:
      "<p>Children in need, episodes of need, referrals and assessments completed by children's social care services.</p>",
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National'],
    file: {
      id: '770a131f-a21f-495f-b1c8-08d987eb5897',
      extension: 'csv',
      fileName: 'a1_cin_referrals_assessments_2013_to_2021.csv',
      name:
        'A1 National time series of children in need, referrals and assessments, England 2013 to 2021',
      size: '15 Kb',
      type: 'Data',
      created: '2021-10-07T16:05:44.1213125Z',
    },
  },
  {
    id: '8bfceddc-e6da-4b26-998e-08d987eb588f',
    name:
      'A2 National time series of section 47s, initial child protection conferences and child protection plans, England 2013 to 2021',
    content:
      '<p>Section 47 enquiries, initial child protection conferences and child protection plans.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National'],
    file: {
      id: 'e1390ee7-5ba0-442d-b1d4-08d987eb5897',
      extension: 'csv',
      fileName: 'a2_section47_icpc_cpp_2013_to_2021.csv',
      name:
        'A2 National time series of section 47s, initial child protection conferences and child protection plans, England 2013 to 2021',
      size: '11 Kb',
      type: 'Data',
      created: '2021-10-08T08:02:58.3152676Z',
    },
  },
  {
    id: '7e47015e-bf0e-4b42-9990-08d987eb588f',
    name:
      'A3 National times series of children in need by gender, age and primary need, England 2018 to 2021',
    content: '<p>Children in need by gender, age and primary need.</p>',
    timePeriods: {
      from: '2018',
      to: '2021',
    },
    geographicLevels: ['National'],
    file: {
      id: '29fd6241-2504-40a8-b1d6-08d987eb5897',
      extension: 'csv',
      fileName: 'a3_cin_primary_need_by_age_gender_2018_to_2021.csv',
      name:
        'A3 National times series of children in need by gender, age and primary need, England 2018 to 2021',
      size: '12 Kb',
      type: 'Data',
      created: '2021-10-08T08:17:41.217646Z',
    },
  },
  {
    id: 'f92c494b-571f-4dda-99a0-08d987eb588f',
    name:
      'A4 National time series of children in need by ethnicity and age and gender, England 2018 to 2021',
    content: '<p>Children in need by gender, age and ethnicity.</p>',
    timePeriods: {
      from: '2018',
      to: '2021',
    },
    geographicLevels: ['National'],
    file: {
      id: '10916365-184e-4c43-b1e6-08d987eb5897',
      extension: 'csv',
      fileName: 'a4_cin_by_ethnicity_and_age_gender_2018_to_2021.csv',
      name:
        'A4 National time series of children in need by ethnicity and age and gender, England 2018 to 2021',
      size: '10 Kb',
      type: 'Data',
      created: '2021-10-08T10:45:35.58381Z',
    },
  },
  {
    id: '2c9b1ba9-b79a-4cba-99a4-08d987eb588f',
    name:
      'A5 National time series of child protection plans by initial category of abuse and gender, age and ethnicity, England 2018 to 2021',
    content:
      '<p>Children who were the subject of a child protection plan at 31 March by initial category of abuse by gender, age and ethnicity.</p>',
    timePeriods: {
      from: '2018',
      to: '2021',
    },
    geographicLevels: ['National'],
    file: {
      id: 'bd56ebb7-0899-43e2-b1ea-08d987eb5897',
      extension: 'csv',
      fileName:
        'a5_cpp_initial_category_of_abuse_by_gender_age_ethnicity_2018_to_2021.csv',
      name:
        'A5 National time series of child protection plans by initial category of abuse and gender, age and ethnicity, England 2018 to 2021',
      size: '6 Kb',
      type: 'Data',
      created: '2021-10-08T11:04:14.9637298Z',
    },
  },
  {
    id: 'c8bd097a-c011-4693-999e-08d987eb588f',
    name:
      'A6 National time series of child protection plans by initial category of abuse, England 2013 to 2021',
    content:
      '<p>Children who were the subject of a child protection plan at 31 March by initial category of abuse.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National'],
    file: {
      id: '0c59a505-593d-44e2-b1e4-08d987eb5897',
      extension: 'csv',
      fileName: 'a6_cpp_initial_category_of_abuse_2013_to_2021.csv',
      name:
        'A6 National time series of child protection plans by initial category of abuse, England 2013 to 2021',
      size: '1 Kb',
      type: 'Data',
      created: '2021-10-08T10:10:01.499493Z',
    },
  },
  {
    id: 'c28569ca-4a59-44cd-9996-08d987eb588f',
    name:
      'A7 National time series of children in need by gender, age and ethnicity, England 2015 to 2021',
    content: '<p>Children in need by gender, age and ethnicity.</p>',
    timePeriods: {
      from: '2015',
      to: '2021',
    },
    geographicLevels: ['National'],
    file: {
      id: '2e8e126a-e3ba-4084-b1dc-08d987eb5897',
      extension: 'csv',
      fileName: 'a7_cin_by_age_gender_ethnicity_2015_to_2021.csv',
      name:
        'A7 National time series of children in need by gender, age and ethnicity, England 2015 to 2021',
      size: '24 Kb',
      type: 'Data',
      created: '2021-10-08T09:58:15.6856206Z',
    },
  },
  {
    id: '0f2f9b9b-85c8-4441-998c-08d987eb588f',
    name:
      'B1 Children in need and episodes of need by local authority, England 2013 to 2021',
    content:
      '<p>Children in need episodes at any point during the year, episodes starting and ending in the year and children in need at 31 March.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '575d47dd-f85f-477e-b1d2-08d987eb5897',
      extension: 'csv',
      fileName: 'b1_children_in_need_2013_to_2021.csv',
      name:
        'B1 Children in need and episodes of need by local authority, England 2013 to 2021',
      size: '292 Kb',
      type: 'Data',
      created: '2021-10-08T07:49:49.8933633Z',
    },
  },
  {
    id: '949f4689-729f-425d-99ac-08d987eb588f',
    name:
      'B2 Children in need at 31 March by recorded disability and local authority, England 2013 to 2021',
    content: '<p>Children in need by disability.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'c4ef4a1b-d8ae-49c3-b1f2-08d987eb5897',
      extension: 'csv',
      fileName: 'b2_children_in_need_recorded_disability_2013_to_2021.csv',
      name:
        'B2 Children in need at 31 March by recorded disability and local authority, England 2013 to 2021',
      size: '264 Kb',
      type: 'Data',
      created: '2021-10-08T13:15:55.3925348Z',
    },
  },
  {
    id: '03027d63-f185-49c8-bccc-08d98e1873d8',
    name:
      'B3 Children in need at 31 March by primary need and local authority, England 2013 to 2021',
    content: '<p>Children in need by primary need at assessment.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'af34ab75-6f70-4b17-420d-08d98e1873fd',
      extension: 'csv',
      fileName: 'b3_cin_primary_need_2013_to_2021.csv',
      name:
        'B3 Children in need at 31 March by primary need and local authority, England 2013 to 2021',
      size: '242 Kb',
      type: 'Data',
      created: '2021-10-14T13:10:24.7844085Z',
    },
  },
  {
    id: 'ef301961-dbfc-44a8-99b0-08d987eb588f',
    name:
      'B4 Children in need at 31 March by duration of episode and local authority, England 2013 to 2021',
    content: '<p>Children in need by duration of episode of need.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'c7445cab-9367-44fe-b1f6-08d987eb5897',
      extension: 'csv',
      fileName: 'b4_children_in_need_duration_of_open_episode_2013_to_2021.csv',
      name:
        'B4 Children in need at 31 March by duration of episode and local authority, England 2013 to 2021',
      size: '254 Kb',
      type: 'Data',
      created: '2021-10-08T13:44:02.0031304Z',
    },
  },
  {
    id: '2ed75fdd-c6b2-433a-99b4-08d987eb588f',
    name:
      'B5 Duration of episodes of need ending in the  year to 31 March by local authority, England 2013 to 2021',
    content:
      '<p>Episodes of need ending during the year by duration of episode of need.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '36b427f3-999b-4367-b1fa-08d987eb5897',
      extension: 'csv',
      fileName:
        'b5_children_in_need_duration_of_ended_episode_2013_to_2021.csv',
      name:
        'B5 Duration of episodes of need ending in the  year to 31 March by local authority, England 2013 to 2021',
      size: '254 Kb',
      type: 'Data',
      created: '2021-10-08T14:28:33.4840179Z',
    },
  },
  {
    id: 'ecd8b9bb-822d-4840-20d1-08d98d84ca80',
    name:
      'B6 Episodes of need ending in the year to 31 March  by reason for closure and local authority, England 2013 to 2021',
    content:
      '<p>Episodes of need ending during the year by reason for case closure.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '0b24fb6b-7dee-46b4-0a9f-08d98d84ca8f',
      extension: 'csv',
      fileName: 'b6_children_in_need_reason_for_closure_2013_to_2021.csv',
      name:
        'B6 Episodes of need ending in the year to 31 March  by reason for closure and local authority, England 2013 to 2021',
      size: '266 Kb',
      type: 'Data',
      created: '2021-10-12T15:25:03.2491626Z',
    },
  },
  {
    id: '655d0d18-d3e4-4c1d-99be-08d987eb588f',
    name:
      "C1 Referrals and re-referrals to children's social care services by local authority, England 2013 to 2021",
    content:
      "<p>Referrals to children's social care services, referrals &nbsp;within 12 months of a previous referral, referrals which resulted in no further action and referrals which were assessed and resulted in no further action.</p>",
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '6cc24d2c-a346-4502-b204-08d987eb5897',
      extension: 'csv',
      fileName:
        'c1_children_in_need_referrals_and_rereferrals_2013_to_2021.csv',
      name:
        "C1 Referrals and re-referrals to children's social care services by local authority, England 2013 to 2021",
      size: '267 Kb',
      type: 'Data',
      created: '2021-10-11T07:54:02.0817658Z',
    },
  },
  {
    id: '84609c65-55d8-49d7-bc5a-08d98e1873d8',
    name:
      'C2 Completed assessments by duration of assessment and local authority, England 2014 to 2021',
    content:
      "<p>Assessments completed by children's social care services by duration of assessment.</p>",
    timePeriods: {
      from: '2014',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'fb9d65f7-41a8-47a2-4198-08d98e1873fd',
      extension: 'csv',
      fileName: 'c2_children_in_need_assessments_duration_2014_to_2021.csv',
      name:
        'C2 Completed assessments by duration of assessment and local authority, England 2014 to 2021',
      size: '229 Kb',
      type: 'Data',
      created: '2021-10-13T09:08:47.3790075Z',
    },
  },
  {
    id: '74eb4c5b-b276-4d28-f2c7-08d99857c8b0',
    name:
      'C3 Factors identified at the end of assessment by local authority, England 2018 to 2021',
    content: '<p>Factors identified at the end of assessment.</p>',
    timePeriods: {
      from: '2018',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '3c23a7b5-42e0-4ac0-ed45-08d99857c8c7',
      extension: 'csv',
      fileName:
        'c3_children_in_need_factors_identified_at_end_of_assessment_2018_to_2021.csv',
      name:
        'C3 Factors identified at the end of assessment by local authority, England 2018 to 2021',
      size: '180 Kb',
      type: 'Data',
      created: '2021-10-26T12:07:38.797588Z',
    },
  },
  {
    id: '432a7ff4-e7ef-46a3-bc5c-08d98e1873d8',
    name:
      'C4 Section 47s and Initial Child Protection Conferences by local authority, England 2013 to 2021',
    content:
      '<p>Section 47 enquiries and initial child protection conferences.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'cea439fd-07bb-4f8a-419a-08d98e1873fd',
      extension: 'csv',
      fileName: 'c4_children_in_need_section_47s_and_icpcs_2013_to_2021.csv',
      name:
        'C4 Section 47s and Initial Child Protection Conferences by local authority, England 2013 to 2021',
      size: '311 Kb',
      type: 'Data',
      created: '2021-10-13T09:28:55.3417685Z',
    },
  },
  {
    id: '7f16dcfc-6491-40ae-bc58-08d98e1873d8',
    name:
      "C5 Referrals to children's social care services by source of referral, England 2014 to 2021",
    content:
      "<p>Referrals to children's social care services by source of referral.</p>",
    timePeriods: {
      from: '2014',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '18dbbfd3-7f22-4889-4196-08d98e1873fd',
      extension: 'csv',
      fileName: 'c5_children_in_need_referrals_by_source_2014_to_2021.csv',
      name:
        "C5 Referrals to children's social care services by source of referral, England 2014 to 2021",
      size: '214 Kb',
      type: 'Data',
      created: '2021-10-13T08:35:21.2321858Z',
    },
  },
  {
    id: 'd843a8fc-0fc3-4e10-bcec-08d98e1873d8',
    name:
      "C6 Referrals to children's social care services by month and local authority, England 2013 to 2021",
    content:
      "<p>Referrals to children's social care services by month of the year.</p>",
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '54461f34-10ff-4e4f-422d-08d98e1873fd',
      extension: 'csv',
      fileName: 'c6_children_in_need_referrals_by_month_2013_to_2021.csv',
      name:
        "C6 Referrals to children's social care services by month and local authority, England 2013 to 2021",
      size: '250 Kb',
      type: 'Data',
      created: '2021-10-14T15:55:47.6658869Z',
    },
  },
  {
    id: '28f10859-b469-494a-99c0-08d987eb588f',
    name: 'D1 Child Protection Plans by local authority, England 2013 to 2021',
    content:
      '<p>Child protection plans starting, ending and at any point during the year and children subject to a child protection plan at 31 March.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '386431a3-30c2-401e-b206-08d987eb5897',
      extension: 'csv',
      fileName: 'd1_child_protection_plans_2013_to_2021.csv',
      name:
        'D1 Child Protection Plans by local authority, England 2013 to 2021',
      size: '271 Kb',
      type: 'Data',
      created: '2021-10-11T08:01:00.6976498Z',
    },
  },
  {
    id: 'b6af0e66-78fc-4a12-bcf2-08d98e1873d8',
    name:
      'D2 Child Protection Plans starting in the year by category of abuse and local authority, England 2013 to 2021',
    content:
      '<p>Child protection plans starting during the year by initial and latest category of abuse.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '011e1d7b-8ac5-44f5-4233-08d98e1873fd',
      extension: 'csv',
      fileName: 'd2_cpps_starting_by_category_of_abuse_2013_to_2021.csv',
      name:
        'D2 Child Protection Plans starting in the year by category of abuse and local authority, England 2013 to 2021',
      size: '289 Kb',
      type: 'Data',
      created: '2021-10-14T16:39:44.5001306Z',
    },
  },
  {
    id: '34c61111-6958-4b47-99c2-08d987eb588f',
    name:
      'D3 Child Protection Plans starting during year which were a second or subsequent plan, by local authority, England 2013 to 2021',
    content:
      '<p>Child protection plans starting in the year and child protection plans which are a second or subsequent plan.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '0fc24e7b-ffc3-4d96-b208-08d987eb5897',
      extension: 'csv',
      fileName: 'd3_cpps_subsequent_plan_2013_to_2021.csv',
      name:
        'D3 Child Protection Plans starting during year which were a second or subsequent plan, by local authority, England 2013 to 2021',
      size: '252 Kb',
      type: 'Data',
      created: '2021-10-11T08:17:02.0521492Z',
    },
  },
  {
    id: '4ab9b578-8326-4c0e-bcf4-08d98e1873d8',
    name:
      'D4 Child Protection Plans at 31 March by category of abuse and local authority, England 2013 to 2021',
    content:
      '<p>Children who were the subject of a child protection plan at 31 March by initial and latest category of abuse.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'e0f88e9e-7ea7-498b-4235-08d98e1873fd',
      extension: 'csv',
      fileName: 'd4_cpps_at31march_by_category_of_abuse_2013_to_2021.csv',
      name:
        'D4 Child Protection Plans at 31 March by category of abuse and local authority, England 2013 to 2021',
      size: '287 Kb',
      type: 'Data',
      created: '2021-10-14T16:46:37.3843777Z',
    },
  },
  {
    id: '71a006ac-9c4e-421d-bc8c-08d98e1873d8',
    name:
      'D5 Child Protection Plans at 31 March by duration and local authority, England 2013 to 2021',
    content:
      '<p>Children who were the subject of a child protection plan at 31 March by duration of the plan.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'eea13b62-93eb-4bc1-41cd-08d98e1873fd',
      extension: 'csv',
      fileName: 'd5_cpps_at31march_by_duration_2013_to_2021.csv',
      name:
        'D5 Child Protection Plans at 31 March by duration and local authority, England 2013 to 2021',
      size: '264 Kb',
      type: 'Data',
      created: '2021-10-13T15:39:18.0125196Z',
    },
  },
  {
    id: '82c7e75b-1317-41a1-bcf6-08d98e1873d8',
    name:
      'D6 Child Protection Plans reviewed within timescales by local authority, England 2013 to 2021',
    content:
      '<p>Children who were the subject of a child protection plan at 31 March that had been on a plan for at least three months and had reviews carried out within the required timescales.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'c27fd5e2-0ead-47c8-4237-08d98e1873fd',
      extension: 'csv',
      fileName: 'd6_cpps_reviewed_within_timescales_2013_to_2021.csv',
      name:
        'D6 Child Protection Plans reviewed within timescales by local authority, England 2013 to 2021',
      size: '237 Kb',
      type: 'Data',
      created: '2021-10-14T16:51:46.5977166Z',
    },
  },
  {
    id: 'cf86bc9a-9d5c-4fd8-bcf0-08d98e1873d8',
    name:
      'D7 Child Protection Plans ending during the year to 31 March by duration and local authority, England 2013 to 2021',
    content:
      '<p>Child protection plans ending during the year by duration of the plan.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: 'b670a401-9586-42b6-4231-08d98e1873fd',
      extension: 'csv',
      fileName: 'd7_cpps_ending_by_duration_2013_to_2021.csv',
      name:
        'D7 Child Protection Plans ending during the year to 31 March by duration and local authority, England 2013 to 2021',
      size: '276 Kb',
      type: 'Data',
      created: '2021-10-14T16:08:31.3655299Z',
    },
  },
  {
    id: '103bab64-2d9f-44b7-bcf8-08d98e1873d8',
    name:
      'D8 Child protection plans ending during the first six months of the year by duration child remained in need and local authority, England 2013 to 2021',
    content:
      '<p>Child protection plans ending during the first six months of the year by duration of the plan.</p>',
    timePeriods: {
      from: '2013',
      to: '2021',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '2a098ea2-1afd-4c5d-4239-08d98e1873fd',
      extension: 'csv',
      fileName: 'd8_cpps_ending_in_first_6months_by_duration_2013_to_2021.csv',
      name:
        'D8 Child protection plans ending during the first six months of the year by duration child remained in need and local authority, England 2013 to 2021',
      size: '296 Kb',
      type: 'Data',
      created: '2021-10-14T16:58:02.1950332Z',
    },
  },
  {
    id: '9b1e3140-5171-4edd-f2dd-08d99857c8b0',
    name: 'Headline Figures',
    content:
      '<p>Children in need, referrals, assessments and child protection plans.</p>',
    timePeriods: {
      from: '2021-22',
      to: '2021-22',
    },
    geographicLevels: ['National'],
    file: {
      id: '2f94053e-e1bf-4df4-ed5c-08d99857c8c7',
      extension: 'csv',
      fileName: 'headline_figures.csv',
      name: 'Headline Figures',
      size: '357 B',
      type: 'Data',
      created: '2021-10-26T14:12:58.7481556Z',
    },
  },
  {
    id: '1e916eb9-6206-46ee-9df5-08d9924737d1',
    name: 'ONS mid-year population estimates',
    content:
      '<p>ONS population estimates for children aged 0 to 17 (age at mid-year).</p>',
    timePeriods: {
      from: '2012',
      to: '2020',
    },
    geographicLevels: ['National', 'Local Authority', 'Regional'],
    file: {
      id: '5fa9ba52-cf83-4973-8738-08d9924737df',
      extension: 'csv',
      fileName: 'ons_mid-year_population_estimates.csv',
      name: 'ONS mid-year population estimates',
      size: '196 Kb',
      type: 'Data',
      created: '2021-10-20T13:35:00.1303415Z',
    },
  },
];

export const summary: PublicationReleaseSummary = {
  id: 'dc190008-a8f7-41a4-faa6-08d973655eae',
  title: 'Reporting Year 2021',
  slug: '2021',
  yearTitle: '2021',
  coverageTitle: 'Reporting Year',
  published: '2021-10-28T08:30:02.6926703',
  releaseName: '2021',
  nextReleaseDate: {
    year: '2022',
    month: '10',
    day: '',
  },
  type: 'NationalStatistics',
  latestRelease: true,
  publication: {
    id: '89869bba-0c00-40f7-b7d6-e28cb904ad37',
    title: 'Characteristics of children in need',
    slug: 'characteristics-of-children-in-need',
  },
};

export const subjectMeta: SubjectMeta = {
  filters: {
    CategoryType: {
      id: '00000000-0000-0000-0000-000000000000',
      legend: 'Category type',
      name: 'category type',
      options: {
        Assessments: {
          id: '00000000-0000-0000-0000-000000000000',
          label: 'Assessments',
          options: [
            {
              label: 'Assessments completed in the year',
              value: '0a19cf09-148f-47f5-8b3e-7ab8c217c234',
            },
          ],
          order: 0,
        },
        ChildrenInNeed: {
          id: '00000000-0000-0000-0000-000000000000',
          label: 'Children in Need',
          options: [
            {
              label:
                'Children awaiting assessment or assessment not required at 31 March',
              value: '10cbad89-8a35-4e72-8567-16aee680fde4',
            },
            {
              label: 'Children ending an episode of need in the year',
              value: '2ca9a79c-fd7d-4c33-8c35-214d992f2cf3',
            },
            {
              label: 'Children in Need at 31 March',
              value: 'e0a3676c-cfc3-403e-8cd9-23d6681dce16',
            },
            {
              label: 'Children in Need at any point during the year',
              value: '42001837-0410-4cc3-91a6-a62c6c14cdcb',
            },
            {
              label: 'Children starting an episode of need in the year',
              value: '1e08840a-ea1d-4880-badf-5638a40f9cc0',
            },
            {
              label: 'Episodes of need at any point in the year',
              value: '713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9',
            },
            {
              label: 'Episodes of need ending in the year',
              value: '3abdf4ce-11b3-493d-b61f-1ea7c9134311',
            },
            {
              label: 'Episodes of need starting in the year',
              value: '2dd980f7-006f-4e47-b564-c2282b83c483',
            },
          ],
          order: 0,
        },
        Referrals: {
          id: '00000000-0000-0000-0000-000000000000',
          label: 'Referrals',
          options: [
            {
              label:
                'Children re-referred within 12 months of a previous referral',
              value: '44235e28-5a14-4c41-a5ad-54bccc304e7d',
            },
            {
              label: 'Children referred in the year',
              value: '09c70424-de6b-46e6-9e3d-3ee3b61a165c',
            },
            {
              label: 'Re-referrals within 12 months of a previous referral',
              value: 'aa10b970-183a-4a65-a684-5a977bcb9c0a',
            },
            {
              label: 'Referral - Assessed as not in Need',
              value: '4cbcb849-bccf-4c84-be6b-b9a2a75548d0',
            },
            {
              label: 'Referral - No Further Action',
              value: '79c184c2-1d56-426a-835b-e42f13544b34',
            },
            {
              label: 'Referrals in the year',
              value: 'f9a82823-7525-4003-bead-681a9f4d71ff',
            },
          ],
          order: 0,
        },
      },
      order: 0,
    },
  },
  indicators: {
    Default: {
      id: '00000000-0000-0000-0000-000000000000',
      label: 'Default',
      options: [
        {
          label: 'Number',
          unit: '',
          value: 'b5781dd6-c1ac-4746-a38e-08d9873287bb',
          name: 'number',
          decimalPlaces: 0,
        },
        {
          label: 'Percentage',
          unit: '',
          value: '855c5779-94c1-4b73-a390-08d9873287bb',
          name: 'percent',
          decimalPlaces: 1,
        },
        {
          label: 'Rate per 10,000 children aged under 18 years',
          unit: '',
          value: '29d8b3d6-c12e-46bd-a38f-08d9873287bb',
          name: 'rate',
          decimalPlaces: 1,
        },
      ],
      order: 0,
    },
  },
  locations: {
    country: {
      legend: 'National',
      options: [
        {
          id: '058416da-0cae-4958-aa00-203d745858ae',
          label: 'England',
          value: 'E92000001',
        },
      ],
    },
  },
  timePeriod: {
    hint: 'Filter statistics by a given start and end date',
    legend: '',
    options: [
      {
        code: 'RY',
        label: '2013',
        year: 2013,
      },
      {
        code: 'RY',
        label: '2014',
        year: 2014,
      },
      {
        code: 'RY',
        label: '2015',
        year: 2015,
      },
      {
        code: 'RY',
        label: '2016',
        year: 2016,
      },
      {
        code: 'RY',
        label: '2017',
        year: 2017,
      },
      {
        code: 'RY',
        label: '2018',
        year: 2018,
      },
      {
        code: 'RY',
        label: '2019',
        year: 2019,
      },
      {
        code: 'RY',
        label: '2020',
        year: 2020,
      },
      {
        code: 'RY',
        label: '2021',
        year: 2021,
      },
    ],
  },
};

export const timePeriodSubjectMeta: SubjectMeta = {
  filters: {},
  indicators: {},
  locations: {},
  timePeriod: {
    hint: 'Filter statistics by a given start and end date',
    legend: '',
    options: [
      {
        code: 'RY',
        label: '2013',
        year: 2013,
      },
      {
        code: 'RY',
        label: '2014',
        year: 2014,
      },
      {
        code: 'RY',
        label: '2015',
        year: 2015,
      },
      {
        code: 'RY',
        label: '2016',
        year: 2016,
      },
      {
        code: 'RY',
        label: '2017',
        year: 2017,
      },
      {
        code: 'RY',
        label: '2018',
        year: 2018,
      },
      {
        code: 'RY',
        label: '2019',
        year: 2019,
      },
      {
        code: 'RY',
        label: '2020',
        year: 2020,
      },
      {
        code: 'RY',
        label: '2021',
        year: 2021,
      },
    ],
  },
};

export const filtersAndIndicatorsSubjectMeta: SubjectMeta = {
  filters: {
    CategoryType: {
      id: '885527ad-80ea-4922-8a39-fbfdf0cf5de7',
      legend: 'Category type',
      options: {
        Assessments: {
          id: 'c4b49777-4330-437e-9623-ad537fb05291',
          label: 'Assessments',
          options: [
            {
              label: 'Assessments completed in the year',
              value: '0a19cf09-148f-47f5-8b3e-7ab8c217c234',
            },
          ],
          order: 0,
        },
        ChildrenInNeed: {
          id: 'e0a791e2-c8ab-498f-9ff1-f5a32d708ea5',
          label: 'Children in Need',
          options: [
            {
              label:
                'Children awaiting assessment or assessment not required at 31 March',
              value: '10cbad89-8a35-4e72-8567-16aee680fde4',
            },
            {
              label: 'Children ending an episode of need in the year',
              value: '2ca9a79c-fd7d-4c33-8c35-214d992f2cf3',
            },
            {
              label: 'Children in Need at 31 March',
              value: 'e0a3676c-cfc3-403e-8cd9-23d6681dce16',
            },
            {
              label: 'Children in Need at any point during the year',
              value: '42001837-0410-4cc3-91a6-a62c6c14cdcb',
            },
            {
              label: 'Children starting an episode of need in the year',
              value: '1e08840a-ea1d-4880-badf-5638a40f9cc0',
            },
            {
              label: 'Episodes of need at any point in the year',
              value: '713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9',
            },
            {
              label: 'Episodes of need ending in the year',
              value: '3abdf4ce-11b3-493d-b61f-1ea7c9134311',
            },
            {
              label: 'Episodes of need starting in the year',
              value: '2dd980f7-006f-4e47-b564-c2282b83c483',
            },
          ],
          order: 1,
        },
        Referrals: {
          id: 'c63dc5c3-a9b4-43ac-9eb9-8bf4c089d6e2',
          label: 'Referrals',
          options: [
            {
              label:
                'Children re-referred within 12 months of a previous referral',
              value: '44235e28-5a14-4c41-a5ad-54bccc304e7d',
            },
            {
              label: 'Children referred in the year',
              value: '09c70424-de6b-46e6-9e3d-3ee3b61a165c',
            },
            {
              label: 'Re-referrals within 12 months of a previous referral',
              value: 'aa10b970-183a-4a65-a684-5a977bcb9c0a',
            },
            {
              label: 'Referral - Assessed as not in Need',
              value: '4cbcb849-bccf-4c84-be6b-b9a2a75548d0',
            },
            {
              label: 'Referral - No Further Action',
              value: '79c184c2-1d56-426a-835b-e42f13544b34',
            },
            {
              label: 'Referrals in the year',
              value: 'f9a82823-7525-4003-bead-681a9f4d71ff',
            },
          ],
          order: 2,
        },
      },
      name: 'category_type',
      order: 0,
    },
  },
  indicators: {
    Default: {
      id: '8c26e631-c519-44e0-b990-129140221126',
      label: 'Default',
      options: [
        {
          label: 'Number',
          unit: '',
          value: 'b5781dd6-c1ac-4746-a38e-08d9873287bb',
          name: 'number',
          decimalPlaces: 0,
        },
        {
          label: 'Percentage',
          unit: '',
          value: '855c5779-94c1-4b73-a390-08d9873287bb',
          name: 'percent',
          decimalPlaces: 1,
        },
        {
          label: 'Rate per 10,000 children aged under 18 years',
          unit: '',
          value: '29d8b3d6-c12e-46bd-a38f-08d9873287bb',
          name: 'rate',
          decimalPlaces: 1,
        },
      ],
      order: 0,
    },
  },
  locations: {},
  timePeriod: {
    legend: '',
    options: [],
  },
};

export const tableData: TableDataResponse = {
  subjectMeta: {
    filters: {
      CategoryType: {
        legend: 'Category type',
        options: {
          Assessments: {
            id: 'c4b49777-4330-437e-9623-ad537fb05291',
            label: 'Assessments',
            options: [
              {
                label: 'Assessments completed in the year',
                value: '0a19cf09-148f-47f5-8b3e-7ab8c217c234',
              },
            ],
            order: 0,
          },
          ChildrenInNeed: {
            id: 'e0a791e2-c8ab-498f-9ff1-f5a32d708ea5',
            label: 'Children in Need',
            options: [
              {
                label:
                  'Children awaiting assessment or assessment not required at 31 March',
                value: '10cbad89-8a35-4e72-8567-16aee680fde4',
              },
              {
                label: 'Children ending an episode of need in the year',
                value: '2ca9a79c-fd7d-4c33-8c35-214d992f2cf3',
              },
              {
                label: 'Children in Need at 31 March',
                value: 'e0a3676c-cfc3-403e-8cd9-23d6681dce16',
              },
              {
                label: 'Children in Need at any point during the year',
                value: '42001837-0410-4cc3-91a6-a62c6c14cdcb',
              },
              {
                label: 'Children starting an episode of need in the year',
                value: '1e08840a-ea1d-4880-badf-5638a40f9cc0',
              },
              {
                label: 'Episodes of need at any point in the year',
                value: '713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9',
              },
              {
                label: 'Episodes of need ending in the year',
                value: '3abdf4ce-11b3-493d-b61f-1ea7c9134311',
              },
              {
                label: 'Episodes of need starting in the year',
                value: '2dd980f7-006f-4e47-b564-c2282b83c483',
              },
            ],
            order: 1,
          },
          Referrals: {
            id: 'c63dc5c3-a9b4-43ac-9eb9-8bf4c089d6e2',
            label: 'Referrals',
            options: [
              {
                label:
                  'Children re-referred within 12 months of a previous referral',
                value: '44235e28-5a14-4c41-a5ad-54bccc304e7d',
              },
              {
                label: 'Children referred in the year',
                value: '09c70424-de6b-46e6-9e3d-3ee3b61a165c',
              },
              {
                label: 'Re-referrals within 12 months of a previous referral',
                value: 'aa10b970-183a-4a65-a684-5a977bcb9c0a',
              },
              {
                label: 'Referral - Assessed as not in Need',
                value: '4cbcb849-bccf-4c84-be6b-b9a2a75548d0',
              },
              {
                label: 'Referral - No Further Action',
                value: '79c184c2-1d56-426a-835b-e42f13544b34',
              },
              {
                label: 'Referrals in the year',
                value: 'f9a82823-7525-4003-bead-681a9f4d71ff',
              },
            ],
            order: 2,
          },
        },
        name: 'category_type',
        order: 0,
      },
    },
    footnotes: [
      {
        id: '2ecbac06-db46-45b1-fbb8-08d99244e018',
        label:
          'Rates per 10,000 of the population of children aged under 18 years are calculated using Office for National Statistics (ONS) mid-year population estimates for children aged 0 to 17 years in England.',
      },
      {
        id: '04ac5718-72db-40a5-c991-08d99853fe7d',
        label:
          'If a child has more than one referral in a reporting year, then each referral is counted.',
      },
      {
        id: '69f1b01d-bba1-41d6-c993-08d99853fe7d',
        label:
          'A child can have more than one episode of need throughout the year, but episodes should not overlap. ',
      },
    ],
    indicators: [
      {
        label: 'Number',
        unit: '',
        value: 'b5781dd6-c1ac-4746-a38e-08d9873287bb',
        name: 'number',
        decimalPlaces: 0,
      },
      {
        label: 'Percentage',
        unit: '',
        value: '855c5779-94c1-4b73-a390-08d9873287bb',
        name: 'percent',
        decimalPlaces: 1,
      },
      {
        label: 'Rate per 10,000 children aged under 18 years',
        unit: '',
        value: '29d8b3d6-c12e-46bd-a38f-08d9873287bb',
        name: 'rate',
        decimalPlaces: 1,
      },
    ],
    locations: {
      country: [
        {
          id: '058416da-0cae-4958-aa00-203d745858ae',
          label: 'England',
          value: 'E92000001',
        },
      ],
    },
    boundaryLevels: [
      {
        id: 1,
        label:
          'Countries December 2017 Ultra Generalised Clipped Boundaries in UK',
      },
    ],
    publicationName: 'Characteristics of children in need',
    subjectName:
      'A1 National time series of children in need, referrals and assessments, England 2013 to 2021',
    timePeriodRange: [
      {
        code: 'RY',
        label: '2013',
        year: 2013,
      },
      {
        code: 'RY',
        label: '2014',
        year: 2014,
      },
      {
        code: 'RY',
        label: '2015',
        year: 2015,
      },
      {
        code: 'RY',
        label: '2016',
        year: 2016,
      },
      {
        code: 'RY',
        label: '2017',
        year: 2017,
      },
      {
        code: 'RY',
        label: '2018',
        year: 2018,
      },
    ],
    geoJsonAvailable: true,
  },
  results: [
    {
      filters: ['713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '753840',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '635.2',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['42001837-0410-4cc3-91a6-a62c6c14cdcb'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '705060',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '594.1',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['2dd980f7-006f-4e47-b564-c2282b83c483'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '406770',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '342.8',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['1e08840a-ea1d-4880-badf-5638a40f9cc0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '382180',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '322.1',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['3abdf4ce-11b3-493d-b61f-1ea7c9134311'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '349130',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '294.2',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['2ca9a79c-fd7d-4c33-8c35-214d992f2cf3'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '331910',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '279.7',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['e0a3676c-cfc3-403e-8cd9-23d6681dce16'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '404710',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '341',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['10cbad89-8a35-4e72-8567-16aee680fde4'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '26130',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '22',
        '855c5779-94c1-4b73-a390-08d9873287bb': '6.5',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['f9a82823-7525-4003-bead-681a9f4d71ff'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '655630',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '552.5',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['aa10b970-183a-4a65-a684-5a977bcb9c0a'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '143810',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '21.9',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['79c184c2-1d56-426a-835b-e42f13544b34'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '61690',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '9.4',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['4cbcb849-bccf-4c84-be6b-b9a2a75548d0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '186560',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '28.5',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['09c70424-de6b-46e6-9e3d-3ee3b61a165c'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '581280',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '489.8',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['44235e28-5a14-4c41-a5ad-54bccc304e7d'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '120190',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '20.7',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['0a19cf09-148f-47f5-8b3e-7ab8c217c234'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '631090',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '531.8',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2018_RY',
    },
    {
      filters: ['713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '742890',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '630.4',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['42001837-0410-4cc3-91a6-a62c6c14cdcb'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '692880',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '587.9',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['2dd980f7-006f-4e47-b564-c2282b83c483'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '400110',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '339.5',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['1e08840a-ea1d-4880-badf-5638a40f9cc0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '374640',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '317.9',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['3abdf4ce-11b3-493d-b61f-1ea7c9134311'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '353860',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '300.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['2ca9a79c-fd7d-4c33-8c35-214d992f2cf3'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '335100',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '284.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['e0a3676c-cfc3-403e-8cd9-23d6681dce16'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '389040',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '330.1',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['10cbad89-8a35-4e72-8567-16aee680fde4'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '23530',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '20',
        '855c5779-94c1-4b73-a390-08d9873287bb': '6',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['f9a82823-7525-4003-bead-681a9f4d71ff'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '646120',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '548.2',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['aa10b970-183a-4a65-a684-5a977bcb9c0a'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '141680',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '21.9',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['79c184c2-1d56-426a-835b-e42f13544b34'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '66040',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '10.2',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['4cbcb849-bccf-4c84-be6b-b9a2a75548d0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '179930',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '27.8',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['09c70424-de6b-46e6-9e3d-3ee3b61a165c'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '571000',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '484.5',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['44235e28-5a14-4c41-a5ad-54bccc304e7d'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '117770',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '20.6',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['0a19cf09-148f-47f5-8b3e-7ab8c217c234'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '606910',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '515',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2017_RY',
    },
    {
      filters: ['713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '747490',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '640.1',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['42001837-0410-4cc3-91a6-a62c6c14cdcb'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '694590',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '594.8',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['2dd980f7-006f-4e47-b564-c2282b83c483'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '401480',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '343.8',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['1e08840a-ea1d-4880-badf-5638a40f9cc0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '373020',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '319.4',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['3abdf4ce-11b3-493d-b61f-1ea7c9134311'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '353590',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '302.8',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['2ca9a79c-fd7d-4c33-8c35-214d992f2cf3'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '331950',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '284.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['e0a3676c-cfc3-403e-8cd9-23d6681dce16'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '393910',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '337.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['10cbad89-8a35-4e72-8567-16aee680fde4'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '27350',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '23.4',
        '855c5779-94c1-4b73-a390-08d9873287bb': '6.9',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['f9a82823-7525-4003-bead-681a9f4d71ff'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '621470',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '532.2',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['aa10b970-183a-4a65-a684-5a977bcb9c0a'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '139900',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '22.5',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['79c184c2-1d56-426a-835b-e42f13544b34'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '61800',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '9.9',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['4cbcb849-bccf-4c84-be6b-b9a2a75548d0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '158060',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '25.4',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['09c70424-de6b-46e6-9e3d-3ee3b61a165c'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '547330',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '468.7',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['44235e28-5a14-4c41-a5ad-54bccc304e7d'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '113820',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '20.8',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['0a19cf09-148f-47f5-8b3e-7ab8c217c234'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '571640',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '489.5',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2016_RY',
    },
    {
      filters: ['713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '754460',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '650.9',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['42001837-0410-4cc3-91a6-a62c6c14cdcb'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '698680',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '602.7',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['2dd980f7-006f-4e47-b564-c2282b83c483'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '403300',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '347.9',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['1e08840a-ea1d-4880-badf-5638a40f9cc0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '373610',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '322.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['3abdf4ce-11b3-493d-b61f-1ea7c9134311'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '364330',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '314.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['2ca9a79c-fd7d-4c33-8c35-214d992f2cf3'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '341030',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '294.2',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['e0a3676c-cfc3-403e-8cd9-23d6681dce16'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '390130',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '336.6',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['10cbad89-8a35-4e72-8567-16aee680fde4'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '27480',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '23.7',
        '855c5779-94c1-4b73-a390-08d9873287bb': '7',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['f9a82823-7525-4003-bead-681a9f4d71ff'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '635620',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '548.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['aa10b970-183a-4a65-a684-5a977bcb9c0a'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '154070',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '24.2',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['79c184c2-1d56-426a-835b-e42f13544b34'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '87530',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '13.8',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['4cbcb849-bccf-4c84-be6b-b9a2a75548d0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '146300',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '23',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['09c70424-de6b-46e6-9e3d-3ee3b61a165c'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '553500',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '477.5',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['44235e28-5a14-4c41-a5ad-54bccc304e7d'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '124060',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '22.4',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['0a19cf09-148f-47f5-8b3e-7ab8c217c234'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '550810',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '475.2',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2015_RY',
    },
    {
      filters: ['713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '767910',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '667.4',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['42001837-0410-4cc3-91a6-a62c6c14cdcb'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '708780',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '616',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['2dd980f7-006f-4e47-b564-c2282b83c483'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '425710',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '370',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['1e08840a-ea1d-4880-badf-5638a40f9cc0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '392880',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '341.4',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['3abdf4ce-11b3-493d-b61f-1ea7c9134311'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '373100',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '324.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['2ca9a79c-fd7d-4c33-8c35-214d992f2cf3'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '347920',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '302.4',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['e0a3676c-cfc3-403e-8cd9-23d6681dce16'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '395480',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '343.7',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['10cbad89-8a35-4e72-8567-16aee680fde4'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '27930',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '24.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': '7.1',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['f9a82823-7525-4003-bead-681a9f4d71ff'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '657780',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '571.7',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['aa10b970-183a-4a65-a684-5a977bcb9c0a'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '154800',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '23.5',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['79c184c2-1d56-426a-835b-e42f13544b34'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '92450',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '14.1',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['4cbcb849-bccf-4c84-be6b-b9a2a75548d0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '128910',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '19.6',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['09c70424-de6b-46e6-9e3d-3ee3b61a165c'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '570790',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '496.1',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['44235e28-5a14-4c41-a5ad-54bccc304e7d'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '124970',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '21.9',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['0a19cf09-148f-47f5-8b3e-7ab8c217c234'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '175290',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '152.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2014_RY',
    },
    {
      filters: ['713eb6ff-e1f0-4f5a-bf08-83d1d4aed1c9'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '728840',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '638',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['42001837-0410-4cc3-91a6-a62c6c14cdcb'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '675030',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '590.9',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['2dd980f7-006f-4e47-b564-c2282b83c483'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '394940',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '345.7',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['1e08840a-ea1d-4880-badf-5638a40f9cc0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '363580',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '318.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['3abdf4ce-11b3-493d-b61f-1ea7c9134311'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '348930',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '305.5',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['2ca9a79c-fd7d-4c33-8c35-214d992f2cf3'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '325850',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '285.3',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['e0a3676c-cfc3-403e-8cd9-23d6681dce16'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '378030',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '330.9',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['10cbad89-8a35-4e72-8567-16aee680fde4'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '19920',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '17.4',
        '855c5779-94c1-4b73-a390-08d9873287bb': '5.3',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['f9a82823-7525-4003-bead-681a9f4d71ff'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '593470',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '519.5',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['aa10b970-183a-4a65-a684-5a977bcb9c0a'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '146770',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '24.7',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['79c184c2-1d56-426a-835b-e42f13544b34'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '85830',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '14.5',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['4cbcb849-bccf-4c84-be6b-b9a2a75548d0'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '112590',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '19',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['09c70424-de6b-46e6-9e3d-3ee3b61a165c'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '511500',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': '447.8',
        '855c5779-94c1-4b73-a390-08d9873287bb': 'z',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['44235e28-5a14-4c41-a5ad-54bccc304e7d'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': '116920',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': 'z',
        '855c5779-94c1-4b73-a390-08d9873287bb': '22.9',
      },
      timePeriod: '2013_RY',
    },
    {
      filters: ['0a19cf09-148f-47f5-8b3e-7ab8c217c234'],
      geographicLevel: 'country',
      locationId: '058416da-0cae-4958-aa00-203d745858ae',
      measures: {
        'b5781dd6-c1ac-4746-a38e-08d9873287bb': ':',
        '29d8b3d6-c12e-46bd-a38f-08d9873287bb': ':',
        '855c5779-94c1-4b73-a390-08d9873287bb': ':',
      },
      timePeriod: '2013_RY',
    },
  ],
};
