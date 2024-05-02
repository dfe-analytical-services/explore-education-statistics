const seoRedirects = [
  {
    from: '/nbsp;andnbsp;https://www.gov.uk/government/collections/abortion-statistics-for-england-and-wales',
    to: '/',
  },
  { from: '/!nd-statistics/permanent-and-', to: '/' },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/7763f949-eff9-4571-93b4-08db2bd4eec9',
    to: '/',
  },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/9348e5bc-c09d-43b4-de90-08d90ae3d820',
    to: '/',
  },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/a095bcaa-7c7c-4a99-de75-08d90ae3d820',
    to: '/',
  },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/cd50d521-0d75-498e-143b-08dac7b96540',
    to: '/',
  },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/d8ecf739-f299-413f-126f-08dacbba9031',
    to: '/',
  },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/ef57d23a-f281-4b6b-a55e-08da6007d7ab',
    to: '/',
  },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/fcfa509c-8572-4b61-ca3a-08dbcefb484c',
    to: '/',
  },
  {
    from: '/api/methodologies/%7BmethodologyId%7D/images/feeffb4e-f022-469a-6e23-08dacc6823fb',
    to: '/',
  },
  {
    from: '/api/releases/%7BreleaseId%7D/images/0eb3fc80-77ad-4d66-8090-a6c2509d50c5',
    to: '/',
  },
  {
    from: '/api/releases/%7BreleaseId%7D/images/26f6ac12-25d0-4831-5293-08db35b5456a',
    to: '/',
  },
  {
    from: '/api/releases/%7BreleaseId%7D/images/fdb52a67-704d-44f3-9721-752000ff9ab9',
    to: '/',
  },
  { from: '/data', to: '/' },
  { from: '/data-', to: '/' },
  { from: '/data-table', to: '/' },
  { from: '/data-tablesnbsp;-nbsp;springnbsp;2022', to: '/' },
  { from: '/data-tables/fast-track/%5BdataBlockId%5D', to: '/' },
  { from: '/data-tables/fast-track/%5BdataBlockParentId%5D', to: '/' },
  { from: '/data-tables/permalink/%5Bpermalink%5D', to: '/data-tables' },
  {
    from: '/data-tables/permalink/09fca49f-7bd8-475f-dd03-08db8e99978c.',
    to: '/data-tables',
  },
  { from: '/data-tables/permalink/167d18fb-', to: '/data-tables' },
  {
    from: '/data-tables/permalink/203a8889-429b-469f-abe1-',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/2e97cf7e-3fe6-4e97-8525-',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/36526263-f93b-4bb1-9d1f-',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/47223cca-6ffb-42da-be50-',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/55ca2487-7a4d-43c4-487d-08db8e99c26c.',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/59fc4e59-160c-4755-c612-08',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/5ab5dbf2-393b-4a43-b7a1-',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/67961e79-5204-410e-b521-',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/85ba124e-1489-4648-bfde-',
    to: '/data-tables',
  },
  {
    from: '/data-tables/permalink/85ba124e-1489-4648-bfde-3eff6fdf76a2;',
    to: '/data-tables',
  },
  { from: '/data-tables/permalink/8f50fce8-', to: '/data-tables' },
  { from: '/data-tables/permalink/970e9bb8-d185-', to: '/data-tables' },
  {
    from: '/data-tables/permalink/9795440a-015e-47eb-86e2-',
    to: '/data-tables',
  },
  { from: '/data-tables/permalink/9e420c30-', to: '/data-tables' },
  {
    from: '/data-tables/permalink/c0319f82-e1e3-4adf-9db1-',
    to: '/data-tables',
  },
  { from: '/data-tables/permalink/c4cb6884-', to: '/data-tables' },
  { from: '/data-tables/permalink/e0980465-', to: '/data-tables' },
  { from: '/data-tables/permalink/e8942369-b2a3-', to: '/data-tables' },
  { from: '/datatables/apprenticeships-and-traineeships', to: '/data-tables' },
  { from: '/datatables/permalink/48f9a035-9123-477e-', to: '/data-tables' },
  { from: '/datatables/permalink/b169759c-', to: '/data-tables' },
  { from: '/find-st%3C/body%3E%3C/html%3E', to: '/find-statistics' },
  { from: '/find-sta', to: '/find-statistics' },
  { from: '/find-stati', to: '/find-statistics' },
  { from: '/find-statis-', to: '/find-statistics' },
  { from: '/find-statistic', to: '/find-statistics' },
  { from: '/find-statistics/[publication]', to: '/find-statistics' },
  { from: '/find-statistics/[publication]/[release]', to: '/find-statistics' },
  { from: '/find-statistics/%3ca%20href=', to: '/find-statistics' },
  { from: '/find-statistics/16-18-', to: '/find-statistics' },
  {
    from: '/find-statistics/16-18-destination-measure',
    to: '/find-statistics',
  },
  { from: '/find-statistics/a-', to: '/find-statistics' },
  { from: '/find-statistics/a-level', to: '/find-statistics' },
  { from: '/find-statistics/a-level-and-other-16-to-', to: '/find-statistics' },
  {
    from: '/find-statistics/a-level-and-other-16-to-18-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/a-level-and-other-16-to-18-results/2019-20&nbsp%e2%80%93',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/a-level-and-other-level-3-results-in-england-academic-year-2019-to-2020',
    to: '/find-statistics',
  },
  { from: '/find-statistics/admission-', to: '/find-statistics' },
  { from: '/find-statistics/apprenticeships-', to: '/find-statistics' },
  { from: '/find-statistics/apprenticeships-and', to: '/find-statistics' },
  {
    from: '/find-statistics/apprenticeships-and-traineeships/2020-21&nbsp;-&nbsp;apprenticeship&nbsp;starts&nbsp;down&nbsp;0.3%&nbsp;in&nbsp;21-22&nbsp;vs&nbsp;19-20',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/apprenticeships-and-traineeshipsThe',
    to: '/find-statistics',
  },
  { from: '/find-statistics/at-', to: '/find-statistics' },
  {
    from: '/find-statistics/attainment-8-score-averages-by-previous-attainment/2019-to-2020-revised',
    to: '/find-statistics',
  },
  { from: '/find-statistics/attendance-', to: '/find-statistics' },
  { from: '/find-statistics/attendance-in-', to: '/find-statistics' },
  { from: '/find-statistics/attendance-in-edu', to: '/find-statistics' },
  { from: '/find-statistics/attendance-in-educa-', to: '/find-statistics' },
  { from: '/find-statistics/attendance-in-education', to: '/find-statistics' },
  { from: '/find-statistics/attendance-in-education-', to: '/find-statistics' },
  {
    from: '/find-statistics/attendance-in-education-and-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-earl',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-settings-during-the-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-settings-during-the-coronavirus-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-settings-during-the-coronavirus-covid-19-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-settings-during-the-coronavirus-covid-19-outbreak&nbsp;2',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-settings-during-the-coronavirus-covid-19-outbreak/2020-week-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-settings-during-the-coronavirus-covid-19-outbreak/2021-week-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-and-early-years-settings-during-the-coronavirus-covid-19-outbreak/2022-week-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/attendance-in-education-andearly-years-settings-during-the-coronavirus-covid-19-outbreak',
    to: '/find-statistics',
  },
  { from: '/find-statistics/attendance-ineducation-', to: '/find-statistics' },
  { from: '/find-statistics/characteristics-of-', to: '/find-statistics' },
  {
    from: '/find-statistics/characteristics-of-children-in-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/characteristics-of-children-in-n',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/characteristics-of-children-in-need/2022&nbsp',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/characteristics-of-children-in-need&nbsp',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/characteristics-of-children-inneed/2020',
    to: '/find-statistics',
  },
  { from: '/find-statistics/childcare', to: '/find-statistics' },
  { from: '/find-statistics/childcare-and-', to: '/find-statistics' },
  { from: '/find-statistics/childcare-and-early-', to: '/find-statistics' },

  { from: '/find-statistics/children', to: '/find-statistics' },
  { from: '/find-statistics/children-', to: '/find-statistics' },

  { from: '/find-statistics/children-loo', to: '/find-statistics' },
  { from: '/find-statistics/children-looked', to: '/find-statistics' },
  { from: '/find-statistics/children-looked-after-in', to: '/find-statistics' },
  {
    from: '/find-statistics/children-looked-after-in-england',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-includ',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-including',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-including-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-including-adopti',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-including-adoptions/2020&nbsp;-&nbsp;dataDownloads-1',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-including-adoptions/2020developmentofattachmentsbetweenolder',
    to: '/find-statistics',
  },
  {
    from: '/Find-Statistics/Children-Looked-after-in-England-Including-Adoptions/2021',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-including-adoptions/2021&nbsp;/l&nbsp;dataDownloads-1',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-looked-after-in-england-including-adoptions/2022.',
    to: '/find-statistics',
  },
  { from: '/find-statistics/children-s-social-work', to: '/find-statistics' },
  {
    from: '/find-statistics/children-s-social-work-workforce-attrition-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/children-s-social-work-workforce/2022&nbsp',
    to: '/find-statistics',
  },
  { from: '/find-statistics/delivery-of-air-', to: '/find-statistics' },
  {
    from: '/find-statistics/early-years-foundation-stage-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/education-', to: '/find-statistics' },
  { from: '/find-statistics/education-and-training-', to: '/find-statistics' },
  {
    from: '/find-statistics/education-and-training-statistics-for-the-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/education-and-training-statistics-for-the-uk/2022;',
    to: '/find-statistics',
  },
  { from: '/find-statistics/education-health-', to: '/find-statistics' },
  {
    from: '/find-statistics/education-health-and-care',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/education-health-and-care-plan',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/education-health-and-care-plans&nbsp;(opens&nbsp;in&nbsp;external&nbsp;window)',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/education-health-and-care-plans/www.gov.uk/courts-tribunals/first-tier-tribunal-specialeducational-needs-and-disability',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/education-provision-children-under-5/2020&nbsp;(universal&nbsp;three&nbsp;and&nbsp;four-year&nbsp;old&nbsp;offer&nbsp;and&nbsp;disadvantaged&nbsp;two&nbsp;year&nbsp;old&nbsp;offer)&nbsp;and&nbsp;https://www.gov.uk/government/statistics/childcare-and-early-years-survey-of-parents-2019',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/elective-home-education/2022-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/free-school-meals-', to: '/find-statistics' },
  { from: '/find-statistics/further-', to: '/find-statistics' },
  { from: '/find-statistics/further-education-and-', to: '/find-statistics' },
  {
    from: '/find-statistics/further-education-outcome-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/further-education-outcome-based-success-mesures',
    to: '/find-statistics',
  },
  { from: '/find-statistics/graduate-', to: '/find-statistics' },
  { from: '/find-statistics/graduate-labour-', to: '/find-statistics' },
  { from: '/find-statistics/graduate-outcomes-', to: '/find-statistics' },
  {
    from: '/find-statistics/he.modelling@education.gov.uk',
    to: '/find-statistics',
  },
  { from: '/find-statistics/higher-', to: '/find-statistics' },
  {
    from: '/find-statistics/higher-level-learners-in-england/2018-19.',
    to: '/find-statistics',
  },
  { from: '/find-statistics/initial-', to: '/find-statistics' },
  { from: '/find-statistics/initial-teacher-train-', to: '/find-statistics' },
  {
    from: '/find-statistics/initial-teacher-training-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/initial-teacher-training-census/2022-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/initial-teacher-training-performance-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/initial-teacher-training-performance-profiles/2019-20.',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/initial-teacher-trainingcensus/2020-21',
    to: '/find-statistics',
  },
  { from: '/find-statistics/key-stage-', to: '/find-statistics' },
  { from: '/find-statistics/key-stage-1-and-phonics-', to: '/find-statistics' },
  {
    from: '/find-statistics/key-stage-1-and-phonics-screening-check-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/key-stage-2-', to: '/find-statistics' },
  {
    from: '/find-statistics/key-stage-2-attainment-national-headlines/2021',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/key-stage-2-attainment-national-headlines/2021-22&nbspAccessed&nbsp31st&nbspJuly&nbsp2022',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/key-stage-2-attainment/2021-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/key-stage-4', to: '/find-statistics' },
  { from: '/find-statistics/key-stage-4-', to: '/find-statistics' },
  {
    from: '/find-statistics/key-stage-4-destination-measures/2018-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/key-stage-4-performance-', to: '/find-statistics' },
  {
    from: '/find-statistics/key-stage-4-performance-revised/2020-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/key-stage-4-performance-revised/2021%e2%80%9322',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/la-and-school-expenditure/2019-2',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/la-and-school-expenditure/2020-21;',
    to: '/find-statistics',
  },
  { from: '/find-statistics/laptops-and-', to: '/find-statistics' },
  {
    from: '/find-statistics/level-2-and-3-attainment-by-youn',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/level-2-and-3-attainment-by-young-people-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/level-2-and-3-attainment-by-young-people-aged-19/2020-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/looked-after-children-aged-16-to-17-in-independent-or-semi-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/multiplication-tables-check-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/national-tutoring-', to: '/find-statistics' },
  { from: '/find-statistics/neet-statistics-', to: '/find-statistics' },
  {
    from: '/find-statistics/outcomes-for-children-in-need-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/outcomes-for-children-in-need-including-c',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/outcomes-for-children-in-needincluding-children-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/outcomes-for-children-in-needincluding-children-looked-after-by-local-authorities-in-england',
    to: '/find-statistics',
  },
  { from: '/find-statistics/parental', to: '/find-statistics' },
  { from: '/find-statistics/participation', to: '/find-statistics' },
  {
    from: '/find-statistics/participation-in-education-and-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/participation-in-education-training-and-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/participation-measures-in-higher-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/permanent-', to: '/find-statistics' },
  { from: '/find-statistics/permanent-and', to: '/find-statistics' },
  { from: '/find-statistics/permanent-and-fixed', to: '/find-statistics' },
  { from: '/find-statistics/permanent-and-fixed-', to: '/find-statistics' },
  { from: '/find-statistics/permanent-and-fixed-p', to: '/find-statistics' },
  {
    from: '/find-statistics/permanent-and-fixed-period-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclu',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclusions-in-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclusions-in-eng',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclusions-in-eng-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclusions-in-england/2018-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclusions-in-england/2018-19yougov.uk',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclusions-in-englandFigures',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-period-exclusions-inengland/2018-19',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-periodexclusions-in-england',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanent-and-fixed-term-exclusions-in-england',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/permanentand-fixed-period-exclusions-in-england',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/postgraduate-initial-teacher-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/progression-to-%3ca&nbsphref==',
    to: '/find-statistics',
  },
  { from: '/find-statistics/progression-to-higher-ed', to: '/find-statistics' },
  { from: '/find-statistics/pupil', to: '/find-statistics' },
  { from: '/find-statistics/pupil-absence-', to: '/find-statistics' },
  { from: '/find-statistics/pupil-absence-in-', to: '/find-statistics' },
  {
    from: '/find-statistics/pupil-absence-in-schools-in-engl',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/pupil-absence-in-schools-in-england-autumn-and-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/pupil-absence-in-schools-in-england-autumn-term/data',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/pupil-absence-in-schools-in-england)',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/pupil-absence-inschools-in-england-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/pupil-attendance', to: '/find-statistics' },
  { from: '/find-statistics/pupil-attendance-in-', to: '/find-statistics' },
  {
    from: '/find-statistics/pupil-attendance-in-schools/2023-7week-29',
    to: '/find-statistics',
  },
  { from: '/find-statistics/school-funding-', to: '/find-statistics' },
  {
    from: '/find-statistics/school-funding-statistics/2021-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/school-placements-', to: '/find-statistics' },
  { from: '/find-statistics/school-pup', to: '/find-statistics' },
  { from: '/find-statistics/school-pupils-', to: '/find-statistics' },
  { from: '/find-statistics/school-pupils-and-', to: '/find-statistics' },
  { from: '/find-statistics/school-pupils-and-thei', to: '/find-statistics' },
  { from: '/find-statistics/school-pupils-and-their-', to: '/find-statistics' },
  {
    from: '/find-statistics/school-pupils-and-their-ch',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-and-their-cha',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-and-their-characteristics;',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-and-their-characteristics/2019-20.',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-and-their-characteristicsa',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-and-their-characteristicshttps:/explore-education-statistics.service.gov.uk/find-statistics/school-pupils-and-their-characteristics',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-and-their-characteristicsm',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-and-theircharacteristics',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupils-andtheir-characteristics',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-pupilsand-their-characteristics',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-workforce-in-englan%3c/p%3e&nbsp;%3cp%3e&nbsp;for&nbsp;more&nbsp;detailed&nbsp;information&nbsp;about&nbsp;the&nbsp;cookies&nbsp;we&nbsp;use,&nbsp;please&nbsp;see&nbsp;our&nbsp;%3ca&nbsp;href=',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-workforce-in-england,',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-workforce-in-englandhttps:/explore-education-statistics.service.gov.uk/find-statistics/school-workforce-in-england',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/school-workforce-inengland',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/schoolworkforce-in-england',
    to: '/find-statistics',
  },
  { from: '/find-statistics/serious-', to: '/find-statistics' },
  {
    from: '/find-statistics/serious-incident-notifications&nbsp',
    to: '/find-statistics',
  },
  { from: '/find-statistics/skills-bootcamps-', to: '/find-statistics' },
  { from: '/find-statistics/special-', to: '/find-statistics' },
  {
    from: '/find-statistics/special-educational-needs-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/special-educational-needs-in-england/2021-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/special-educational-needs-in-england/2021-2',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/special-educational-needs-in-england/221-22',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/special-educationalneeds-in-england',
    to: '/find-statistics',
  },
  { from: '/find-statistics/student-', to: '/find-statistics' },
  {
    from: '/find-statistics/student-loan-forecasts-for',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/student-loan-forecasts-for-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/the-link-between-absence-and-attainment-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/the-link-between-absence-and-attainment-at-ks2-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/uk-revenue-from-education-related-exports-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/uk-revenue-from-education-related-exports-and-',
    to: '/find-statistics',
  },
  { from: '/find-statistics/widen-', to: '/find-statistics' },
  { from: '/find-statistics/widening', to: '/find-statistics' },
  { from: '/find-statistics/widening-', to: '/find-statistics' },
  {
    from: '/find-statistics/widening-participation-in-higher-',
    to: '/find-statistics',
  },
  {
    from: '/find-statistics/www.gov.uk/courts-tribunals/first-tier-tribunal-specialeducational-needs-and-disability',
    to: '/find-statistics',
  },
  {
    from: '/findstatistics/apprenticeships-and-traineeships/2021-22',
    to: '/find-statistics',
  },
  {
    from: '/findstatistics/apprenticeships-in-england-by-industry-characteristics',
    to: '/find-statistics',
  },
  { from: '/findstatistics/attendance-in-', to: '/find-statistics' },
  {
    from: '/findstatistics/attendance-in-education-and-early-years-',
    to: '/find-statistics',
  },
  {
    from: '/findstatistics/children-s-social-work-workforce',
    to: '/find-statistics',
  },
  {
    from: '/findstatistics/education-and-training-statistics-for-the-uk/2020',
    to: '/find-statistics',
  },
  { from: '/findstatistics/leo-graduate-and', to: '/find-statistics' },
  {
    from: '/findstatistics/special-educational-needs-in-england',
    to: '/find-statistics',
  },
  { from: '/methodol', to: '/methodology' },
  { from: '/methodology/16-18-destination-', to: '/methodology' },
  { from: '/methodology/attendance-in-', to: '/methodology' },
  {
    from: '/methodology/attendance-ineducation-and-early-years-settings-during-the-coronavirus-covid-19-outbreakmethodology',
    to: '/methodology',
  },
  { from: '/methodology/children-looked-after-', to: '/methodology' },
  {
    from: '/methodology/permanent-and-fixed-period-exclusions-in-england',
    to: '/methodology',
  },
  { from: '/methodology/secondary-and-primary-school-', to: '/methodology' },
  {
    from: '/methodology/student-loan-forecasts-for-england-',
    to: '/methodology',
  },
  { from: '/methodology/widening-', to: '/methodology' },
  {
    from: '/methodology/widening-participation-in-higher-',
    to: '/methodology',
  },
  { from: '/service', to: '/' },
  {
    from: '/statistics/pupil-absence-in-schools-in-england',
    to: '/find-statistics',
  },
];

module.exports = seoRedirects;
