import classNames from 'classnames';
import React from 'react';
import styles from '../PrototypePublicPage.module.scss';

interface Props {
  headlines?: boolean;
  hideLink?: boolean;
}

const DashboardContent = ({ headlines, hideLink }: Props) => {
  return (
    <div className={styles.releaseMainContent}>
      {!headlines && (
        <>
          <h2 id="children-early-years">Childcare and early years</h2>

          <div className={styles.prototypeCardDashboardContainerGrid}>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>38,800</em> children aged under 5 in <em>385</em>{' '}
                state-funded nurseries
              </p>

              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/school-pupils-and-their-characteristics"
              >
                Schools, pupils and their characteristics
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                Almost <em>1 in 10</em> (8.6%) nursery pupils are eligible for
                free school meals
              </p>

              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/school-pupils-and-their-characteristics"
              >
                Schools, pupils and their characteristics
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>1.7 million</em> children under 5 benefit from
                government-funded early education and childcare.
              </p>

              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/education-provision-children-under-5"
              >
                Education provision: children under 5
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>1,100</em> nursery teachers means there are <em>23.4</em>{' '}
                pupils per teacher
              </p>

              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/school-workforce-in-england"
              >
                Teachers and school workforce
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>63.4%</em> reception pupils meet the expected level of
                development
              </p>

              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/early-years-foundation-stage-profile-results"
              >
                Early years foundation stage profile reults
              </a>
            </div>
          </div>

          <a href="https://explore-education-statistics.service.gov.uk/find-statistics?themeId=e6e31160-fe79-4556-f3a9-08d86094b9e8&sortBy=newest">
            View all publications in 'Children and early years'
          </a>
          <hr className="govuk-!-margin-top-9" />

          <h2 id="schools">Primary and secondary schools</h2>

          <div className={styles.prototypeCardDashboardContainerGrid}>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>9 million</em> pupils in <em>24,500</em> schools
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/school-pupils-and-their-characteristics"
              >
                Schools, pupils and their characteristics
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>6.4%</em> school sessions missed due to absence this year,
                of which <em>2.0%</em> was unauthorised
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/pupil-absence-in-schools-in-england"
              >
                Pupil absence
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>26.7</em> pupils in each infant class, <em>26.6</em>{' '}
                primary, <em>22.3</em> secondary.
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/school-pupils-and-their-characteristics"
              >
                Schools, pupils and their characteristics
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>465,500</em> teachers and <em>37,500</em> new trainee
                teachers
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/school-workforce-in-england"
              >
                Teachers and school workforce
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>59%</em> pupils meet expected standards of reading, writing
                ad maths at the end of KS2{' '}
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/key-stage-2-attainment"
              >
                Key stage 2 attainment
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>1 in 10</em>(12.6%) pupils receive SEN support.{' '}
                <em>1 in 25</em>
                (4.0%) have an EHC plan
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/special-educational-needs-in-england"
              >
                Special educational needs
              </a>
            </div>
          </div>

          <a href="https://explore-education-statistics.service.gov.uk/find-statistics?themeId=ee1855ca-d1e1-4f04-a795-cbd61d326a1f&sortBy=newest">
            View all publications in 'Primary and secondary schools'
          </a>
          <hr className="govuk-!-margin-top-9" />

          <h2 id="destinations">Destinations after school</h2>

          <div className={styles.prototypeCardDashboardContainerGrid}>
            <div className={styles.prototypeDashboardCard}>
              <p>
                At KS5, the average A level result is <em>B</em>, applied
                general is <em>Dist -</em> and tech level is <em>Dist -</em>
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/a-level-and-other-16-to-18-results"
              >
                A level and other 16 to 18 results
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>1 in 10</em> people aged 16-24 are not in education,
                employment or training
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/participation-in-education-and-training-and-employment"
              >
                NEET and participation
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                Over <em>1 million</em> adults (19+) are in government funded
                further education and skills learning
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/further-education-and-skills#"
              >
                Further education and skills
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>2 in 3</em> pupils who undertook A levels, applied general
                or tech levels went on to higher / further education
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/progression-to-higher-education-or-training"
              >
                Progession to higher or further education
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>740,400</em> people are undertaking an apprenticeship
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships"
              >
                Apprenticeships and traineeships
              </a>
            </div>
            <div className={styles.prototypeDashboardCard}>
              <p>
                Graduates earn, on average <em>&pound;28,200</em> five years
                after graduation, and <em>86.8%</em> are in sustained employment
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/leo-graduate-and-postgraduate-outcomes"
              >
                LEO graduate and postgrduate outcomes
              </a>
            </div>
          </div>

          <a href="https://explore-education-statistics.service.gov.uk/find-statistics?themeId=6412a76c-cf15-424f-8ebc-3a530132b1b3&sortBy=newest">
            View all publications in 'Destinations after school'
          </a>
          <hr className="govuk-!-margin-top-9" />

          <h2 id="children-social-care">Children's social care</h2>
          <div className={styles.prototypeCardDashboardContainerGrid}>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>334</em> per <em>10,000</em> children are children in need
                (CIN)
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/characteristics-of-children-in-need"
              >
                Characteristics of children in need
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>70</em> per <em>10,000</em> children are looked after (CLA)
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/children-looked-after-in-england-including-adoptions"
              >
                Children looked after
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>165</em> children accomodated in secure children's homes
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/children-accommodated-in-secure-childrens-homes"
              >
                Children accommodated in secure children's homes
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>32,500</em> children and family social workers
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/children-s-social-work-workforce-attrition-caseload-and-agency-workforce"
              >
                Children's social work workforce
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>442</em> serious incident notifications
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/serious-incident-notifications"
              >
                Serious incident notifications
              </a>
            </div>
          </div>
          <a href="https://explore-education-statistics.service.gov.uk/find-statistics?themeId=cc8e02fd-5599-41aa-940d-26bca68eab53&sortBy=newest">
            View all publications in 'Children's social care'
          </a>
          <hr className="govuk-!-margin-top-9" />
        </>
      )}

      {headlines && (
        <>
          <div className={styles.prototypeCardDashboardContainerGrid}>
            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>1.7 million</em> children under 5 benefit from
                government-funded early education and childcare.
              </p>

              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/education-provision-children-under-5"
              >
                Education provision: children under 5
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                <em>9 million</em> pupils in <em>24,500</em> schools
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/school-pupils-and-their-characteristics"
              >
                Schools, pupils and their characteristics
              </a>
            </div>

            <div className={styles.prototypeDashboardCard}>
              <p>
                Over <em>1 million</em> adults (19+) are in government funded
                further education and skills learning
              </p>
              <a
                className={classNames(
                  'govuk-link--no-visited-state',
                  styles.prototypeDashboardCardLink,
                )}
                href="https://explore-education-statistics.service.gov.uk/find-statistics/further-education-and-skills#"
              >
                Further education and skills
              </a>
            </div>
          </div>
          {!hideLink && (
            <a href="/prototypes/dashboard2">
              View all headlines from education in numbers
            </a>
          )}
        </>
      )}
    </div>
  );
};

export default DashboardContent;
