/* tslint:disable:object-literal-sort-keys */

export const pupilAbsenceAttributes = {
  Absence: [
    {
      value: 'num_schools',
      label: 'Number of schools',
    },
    {
      value: 'enrolments',
      label: 'Number of pupil enrolments',
    },
    {
      value: 'sess_possible',
      label: 'Number of sessions possible',
    },
    {
      value: 'sess_overall',
      label: 'Number of overall absence sessions',
    },
    {
      value: 'sess_authorised',
      label: 'Number of authorised absence sessions',
    },
    {
      value: 'sess_unauthorised',
      label: 'Number of unauthorised absence sessions',
    },
    {
      value: 'sess_overall_percent',
      label: 'Overall absence rate',
    },
    {
      value: 'sess_authorised_percent',
      label: 'Authorised absence rate',
    },
    {
      value: 'sess_unauthorised_percent',
      label: 'Unauthorised absence rate',
    },
    {
      value: 'enrolments_PA_10_exact',
      label: 'Number of persistent absentees',
    },
    {
      value: 'enrolments_pa_10_exact_percent',
      label: 'Percentage of persistent absentees',
    },
  ],
  'Absence for persistent absentees': [
    {
      value: 'sess_possible_pa_10_exact',
      label: 'Number of sessions possible for persistent absentees',
    },
    {
      value: 'sess_overall_pa_10_exact',
      label: 'Number of overall absence sessions for persistent absentees',
    },
    {
      value: 'sess_authorised_pa_10_exact',
      label: 'Number of authorised absence sessions for persistent absentees',
    },
    {
      value: 'sess_unauthorised_pa_10_exact',
      label: 'Number of unauthorised absence sessions for persistent absentees',
    },
    {
      value: 'sess_overall_percent_pa_10_exact',
      label: 'Overall absence rate for persistent absentees',
    },
    {
      value: 'sess_authorised_percent_pa_10_exact',
      label: 'Authorised absence rate for persistent absentees',
    },
    {
      value: 'sess_unauthorised_percent_pa_10_exact',
      label: 'Unauthorised absence rate for persistent absentees',
    },
  ],
  'Absence by reason': [
    {
      value: 'sess_auth_illness',
      label: 'Number of illness sessions',
    },
    {
      value: 'sess_auth_appointments',
      label: 'Number of medical appointments sessions',
    },
    {
      value: 'sess_auth_religious',
      label: 'Number of religious observance sessions',
    },
    {
      value: 'sess_auth_study',
      label: 'Number of study leave sessions',
    },
    {
      value: 'sess_auth_traveller',
      label: 'Number of traveller sessions',
    },
    {
      value: 'sess_auth_holiday',
      label: 'Number of authorised holiday sessions',
    },
    {
      value: 'sess_auth_ext_holiday',
      label: 'Number of extended authorised holiday sessions',
    },
    {
      value: 'sess_auth_excluded',
      label: 'Number of excluded sessions',
    },
    {
      value: 'sess_auth_other',
      label: 'Number of authorised other sessions',
    },
    {
      value: 'sess_auth_totalreasons',
      label: 'Number of authorised reasons sessions',
    },
    {
      value: 'sess_unauth_holiday',
      label: 'Number of unauthorised holiday sessions',
    },
    {
      value: 'sess_unauth_late',
      label: 'Number of late sessions',
    },
    {
      value: 'sess_unauth_other',
      label: 'Number of unauthorised other sessions',
    },
    {
      value: 'sess_unauth_noyet',
      label: 'Number of no reason yet sessions',
    },
    {
      value: 'sess_unauth_totalreasons',
      label: 'Number of unauthorised reasons sessions',
    },
    {
      value: 'sess_overall_totalreasons',
      label: 'Number of overall reasons sessions',
    },
  ],
};

export const ungroupedPupilAbsenceAttributes: {
  [attribute: string]: string;
} = Object.values(pupilAbsenceAttributes)
  .flatMap(groups => groups)
  .reduce((acc, attribute) => {
    return {
      ...acc,
      [attribute.value]: attribute.label,
    };
  }, {});
