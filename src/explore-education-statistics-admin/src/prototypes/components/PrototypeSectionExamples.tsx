import classNames from 'classnames';
import React from 'react';
import Details from '@common/components/Details';
import PrototypeChartExamples from '@admin/prototypes/components/PrototypeChartExamples';
import stylesKeyStatTile from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import Button from '@common/components/Button';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import PrototypeDashboardContent from '@admin/prototypes/components/PrototypeDashboardContent';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import styles from '../PrototypePublicPage.module.scss';

interface Props {
  sectionExample?: string;
  change?: () => void;
  change2?: () => void;
  change3?: () => void;
}

const ExampleSection = ({
  sectionExample,
  change,
  change2,
  change3,
}: Props) => {
  const [showHelp1, setShowHelp1] = useToggle(false);
  const [showHelp2, setShowHelp2] = useToggle(false);
  const [showHelp3, setShowHelp3] = useToggle(false);
  const [showReleaseTypeModal, toggleReleaseTypeModal] = useToggle(false);
  return (
    <>
      {sectionExample === 'header1' && (
        <>
          <span className="govuk-caption-xl" data-testid="page-title-caption">
            Academic Year 2021/22 TEST1
          </span>
          <h1 className="govuk-heading-xl" data-testid="page-title">
            Apprenticeships and traineeships
          </h1>
          <span className="govuk-tag govuk-!-margin-bottom-3">Latest data</span>
          <div className="dfe-flex">
            <div>
              <div className="govuk-body-l">
                Apprenticeship and traineeship starts, achievements and
                participation. Includes breakdowns by age, sex, ethnicity,
                subject, provider, geography etc.
              </div>
              <a href="#" className="govuk-button">
                Download all data from this release (zip)
              </a>
            </div>

            <img
              src="/assets/images/UKSA-quality-mark.jpg"
              className="govuk-!-margin-right-3"
              alt="UK statistics authority quality mark"
              height="60"
              width="60"
            />
          </div>
          <hr className="govuk-!-margin-bottom-6" />
        </>
      )}
      {sectionExample === 'header2' && (
        <>
          <span className="govuk-caption-xl" data-testid="page-title-caption">
            Academic Year 2021/22 TEST1
          </span>
          <h1 className="govuk-heading-xl" data-testid="page-title">
            Apprenticeships and traineeships
          </h1>
          <div className={styles.releaseContainer}>
            <div className={styles.releaseNav}>
              <ul className="govuk-list govuk-list--spaced govuk-body-s">
                <li>
                  Published <strong>01 October 2023</strong>
                </li>

                <li>
                  <a href="#">National data</a>
                </li>
                <li>
                  <a href="#">Sign up for email updates</a>
                </li>
                <li>
                  <Details
                    summary="Methodologies"
                    className="govuk-!-margin-0 govuk-!-margin-top-6"
                  >
                    Methodologies
                  </Details>
                </li>

                <li>
                  <Details
                    summary="View updates (5)"
                    className="govuk-!-margin-0"
                  >
                    Latest updates
                  </Details>
                </li>
                <li>
                  <Details
                    summary="Releases in this series"
                    className="govuk-!-margin-0"
                  >
                    Releases in this series
                  </Details>
                </li>
              </ul>
            </div>
            <div className={styles.releaseMainContent}>
              <span className="govuk-tag govuk-!-margin-bottom-3">
                Latest data
              </span>
              <div className="dfe-flex">
                <div>
                  <div className="govuk-body-l">
                    Apprenticeship and traineeship starts, achievements and
                    participation. Includes breakdowns by age, sex, ethnicity,
                    subject, provider, geography etc.
                  </div>
                  <a href="#" className="govuk-button">
                    Download all data from this release (zip)
                  </a>
                </div>

                <img
                  src="/assets/images/UKSA-quality-mark.jpg"
                  className="govuk-!-margin-right-3"
                  alt="UK statistics authority quality mark"
                  height="60"
                  width="60"
                />
              </div>
            </div>
          </div>

          <hr className="govuk-!-margin-bottom-6" />
        </>
      )}
      {sectionExample === 'header3' && (
        <>
          <span className="govuk-caption-xl" data-testid="page-title-caption">
            Academic Year 2021/22 TEST1
          </span>
          <h1 className="govuk-heading-xl" data-testid="page-title">
            Apprenticeships and traineeships
          </h1>
          <div className={styles.releaseContainer}>
            <div className={styles.releaseNav}>
              <ul className="govuk-list govuk-list--spaced govuk-body-s">
                <li>
                  Published <strong>01 October 2023</strong>
                </li>
                <li>
                  Next update <strong>01 March 2024</strong>
                </li>
                <li>
                  <a href="#">National statistics</a>
                </li>
                <li>
                  <a href="#">Data guidance</a>
                </li>
                <li>
                  <a href="#">Pre-release access list</a>
                </li>
                <li>
                  <a href="#">
                    <strong>Sign up for email updates</strong>
                  </a>
                </li>
              </ul>
              <a href="#" className="govuk-button govuk-!-margin-top-3">
                Download all data (zip)
              </a>
            </div>
            <div className={styles.releaseMainContent}>
              <div>
                <span className="govuk-tag govuk-!-margin-bottom-3">
                  Latest data
                </span>
              </div>

              <div className="dfe-flex">
                <div>
                  <div className="govuk-body-l">
                    Apprenticeship and traineeship starts, achievements and
                    participation. Includes breakdowns by age, sex, ethnicity,
                    subject, provider, geography etc.
                  </div>
                </div>

                <img
                  src="/assets/images/UKSA-quality-mark.jpg"
                  className="govuk-!-margin-right-3"
                  alt="UK statistics authority quality mark"
                  height="60"
                  width="60"
                />
              </div>
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-full">
                  <Details summary="Methodologies" className="govuk-!-margin-0">
                    Methodologies
                  </Details>
                </div>
                <div className="govuk-grid-column-full">
                  <Details
                    summary="Releases in this series (10)"
                    className="govuk-!-margin-0"
                  >
                    Releases in this series (10)
                  </Details>
                </div>
                <div className="govuk-grid-column-full">
                  <Details
                    summary="Last updated 31 October 2023 (2)"
                    className="govuk-!-margin-0"
                  >
                    Updated 31 October 2023 (2)
                  </Details>
                </div>
              </div>
            </div>
          </div>

          <hr className="govuk-!-margin-bottom-6" />
        </>
      )}
      {sectionExample === 'header4' && (
        <>
          <span className="govuk-caption-xl" data-testid="page-title-caption">
            Academic Year 2021/22 TEST1
          </span>
          <h1 className="govuk-heading-xl" data-testid="page-title">
            Apprenticeships and traineeships
          </h1>
          <div className={styles.releaseContainer}>
            <div className={styles.releaseNav}>
              <h2 className="govuk-body">Related information</h2>
              <ul className="govuk-list govuk-list--spaced govuk-!-margin-top-3">
                <li>
                  <a href="#">Data guidance</a>
                </li>
                <li>
                  <a href="#">Pre-release contact list</a>
                </li>
                <li>
                  <a href="#">
                    <strong>Sign up for email updates</strong>
                  </a>
                </li>
              </ul>
              <h3 className="govuk-body">Methodologies</h3>
              <ul className="govuk-list govuk-list--spaced govuk-!-margin-top-3">
                <li>
                  <a href="#">
                    Further education and skills statistics: methodology
                  </a>
                </li>
              </ul>
            </div>
            <div className={styles.releaseMainContent}>
              <div className="dfe-flex">
                <div>
                  <div>
                    <span className="govuk-tag govuk-!-margin-bottom-3">
                      Latest data
                    </span>
                  </div>
                  <span>
                    Published <strong>01 October 2023</strong>
                  </span>

                  <p className="govuk-!-margin-0 govuk-!-margin-bottom-2">
                    Next release <strong>01 October 2024</strong>
                  </p>
                  <a href="#">National statistics</a>
                  <div className="govuk-body govuk-!-margin-top-3">
                    Apprenticeship and traineeship starts, achievements and
                    participation. Includes breakdowns by age, sex, ethnicity,
                    subject, provider, geography etc.
                  </div>
                </div>

                <img
                  src="/assets/images/UKSA-quality-mark.jpg"
                  className="govuk-!-margin-right-3"
                  alt="UK statistics authority quality mark"
                  height="60"
                  width="60"
                />
              </div>

              <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                <a href="#" className="govuk-button govuk-!-margin-0">
                  Download all data from this release (zip)
                </a>
                <div>
                  <Details
                    summary="Last updated 31 October 2023"
                    className="govuk-!-margin-0"
                  >
                    Updated 31 October 2023 (2)
                  </Details>
                  <Details
                    summary="Releases in this series (2)"
                    className="govuk-!-margin-0"
                  >
                    Updated 31 October 2023 (2)
                  </Details>
                </div>

                {/* 
                <div>
                  <Details summary="Methodologies" className="govuk-!-margin-0">
                    Updated 31 October 2023 (2)
                  </Details>
                  <Details
                    summary="Last updated 31 October 2023"
                    className="govuk-!-margin-0"
                  >
                    Updated 31 October 2023 (2)
                  </Details>
                  <Details
                    summary="Releases in this series (2)"
                    className="govuk-!-margin-0"
                  >
                    Updated 31 October 2023 (2)
                  </Details>
                </div>
                */}
              </div>
            </div>
          </div>

          <hr className="govuk-!-margin-bottom-6" />
        </>
      )}
      {sectionExample === 'header5' && (
        <>
          <div className={styles.releaseContainer}>
            <div className={styles.releaseMainContent}>
              <span
                className="govuk-caption-xl"
                data-testid="page-title-caption"
              >
                Academic Year 2021/22 TEST1
              </span>
              <h1 className="govuk-heading-xl" data-testid="page-title">
                Apprenticeships and traineeships
              </h1>
              <span>
                Published <strong>01 October 2023</strong>
              </span>
              <span className="govuk-tag govuk-!-margin-bottom-3 govuk-!-margin-left-3">
                Latest data
              </span>
              <div className="dfe-flex">
                <div>
                  <div className="govuk-body-l">
                    Apprenticeship and traineeship starts, achievements and
                    participation. Includes breakdowns by age, sex, ethnicity,
                    subject, provider, geography etc.
                  </div>
                </div>

                <img
                  src="/assets/images/UKSA-quality-mark.jpg"
                  className="govuk-!-margin-right-3"
                  alt="UK statistics authority quality mark"
                  height="60"
                  width="60"
                />
              </div>
              <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                <a href="#" className="govuk-button govuk-!-margin-0">
                  Download all data from this release (zip)
                </a>
              </div>
            </div>
            <div
              className={styles.releaseNav}
              style={{ marginLeft: '1rem', marginTop: '2rem' }}
            >
              <h2 className="govuk-body">Related information</h2>
              <ul className="govuk-list govuk-list--spaced govuk-body-s">
                <li>
                  <a href="#">National data</a>
                </li>
                <li>
                  <a href="#">Methodology</a>
                </li>
                <li>
                  <a href="#">Data guidance</a>
                </li>
                <li>
                  <a href="#">Pre-release contact list</a>
                </li>
                <li>
                  <a href="#">Sign up for email updates</a>
                </li>
              </ul>
            </div>
          </div>

          <hr className="govuk-!-margin-bottom-6" />
        </>
      )}
      {sectionExample === 'header6' && (
        <>
          <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
            <div>
              <span
                className="govuk-caption-xl dfe-flex dfe-align-items--center"
                data-testid="page-title-caption"
              >
                <span className="govuk-tag govuk-!-margin-right-2">
                  Latest data
                </span>{' '}
                Academic Year 2021/22
              </span>
              <h1
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                data-testid="page-title"
              >
                Apprenticeships and traineeships
              </h1>
              <p className="govuk-body-s govuk-!-margin-bottom-0">
                Published 01 October 2023
              </p>
              <p className="govuk-body-s">
                Last updated 31 October 2023{' '}
                <a href="#" className="govuk-link--no-underline">
                  view updates
                </a>
              </p>
            </div>
            <img
              src="/assets/images/UKSA-quality-mark.jpg"
              className="govuk-!-margin-right-3"
              alt="UK statistics authority quality mark"
              height="60"
              width="60"
            />
          </div>

          <div className="dfe-flex">
            <div>
              <div className="govuk-body-l">
                Apprenticeship and traineeship starts, achievements and
                participation. Includes breakdowns by age, sex, ethnicity,
                subject, provider, geography etc.
              </div>
            </div>
            <div className={styles.prototypeHeaderLinks}>
              <ul className="govuk-list">
                <li>
                  <a href="#">Download all data (ZIP)</a>
                </li>
                <li>
                  <a href="#">Sign up for email updates</a>
                </li>
              </ul>
            </div>
          </div>

          <hr className="govuk-!-margin-bottom-6" />
        </>
      )}
      {sectionExample === 'header7' && (
        <>
          <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
            <div>
              <span
                className="govuk-caption-xl dfe-flex dfe-align-items--center"
                data-testid="page-title-caption"
              >
                <span className="govuk-tag govuk-!-margin-right-2">
                  Latest data
                </span>{' '}
                Academic Year 2021/22
              </span>
              <h1
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                data-testid="page-title"
              >
                Apprenticeships and traineeships
              </h1>

              <p className="govuk-body-s govuk-!-margin-bottom-0">
                Published 01 October 2023
              </p>
              <p className="govuk-body-s">
                Last updated 31 October 2023,{' '}
                <a href="#releaseDetails" className="govuk-link--no-underline">
                  see all updates
                </a>
              </p>
            </div>
            <img
              src="/assets/images/UKSA-quality-mark.jpg"
              className="govuk-!-margin-right-3"
              alt="UK statistics authority quality mark"
              height="60"
              width="60"
            />
          </div>

          <div className="dfe-flex">
            <div>
              <div className="govuk-body">
                Apprenticeship and traineeship starts, achievements and
                participation. Includes breakdowns by age, sex, ethnicity,
                subject, provider, geography etc.
              </div>
            </div>
          </div>

          <section className="govuk-!-margin-bottom-6">
            <h2 className="govuk-body">Quicklinks</h2>
            <ul
              className={classNames(styles.prototypeQuickLinks, 'govuk-list')}
            >
              <li>
                <div className={styles.prototypeActionLink}>
                  <span className={styles.prototypeActionLinkWrapper}>
                    <a href="#">Download all data (ZIP)</a>
                  </span>
                </div>
              </li>
              <li>
                <div className={styles.prototypeActionLink}>
                  <span className={styles.prototypeActionLinkWrapper}>
                    <a href="#releaseDetails">Methodologies</a>
                  </span>
                </div>
              </li>
              <li>
                <div className={styles.prototypeActionLink}>
                  <span className={styles.prototypeActionLinkWrapper}>
                    <a href="#">Data guidance</a>
                  </span>
                </div>
              </li>
              <li>
                <div className={styles.prototypeActionLink}>
                  <span className={styles.prototypeActionLinkWrapper}>
                    <a href="#releaseDetails">Other releases in this series</a>
                  </span>
                </div>
              </li>
              <li>
                <div className={styles.prototypeActionLink}>
                  <span className={styles.prototypeActionLinkWrapper}>
                    <a href="#help">Help, support and contact</a>
                  </span>
                </div>
              </li>
              <li>
                <div className={styles.prototypeActionLink}>
                  <span className={styles.prototypeActionLinkWrapper}>
                    <a href="#">Sign up for email alerts</a>
                  </span>
                </div>
              </li>
            </ul>
          </section>
        </>
      )}
      {sectionExample === 'header8' && (
        <>
          <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
            <div>
              <span
                className="govuk-caption-xl dfe-flex dfe-align-items--center"
                data-testid="page-title-caption"
              >
                <span className="govuk-tag govuk-!-margin-right-2">
                  Latest data
                </span>{' '}
                Academic Year 2021/22
              </span>
              <h1
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                data-testid="page-title"
              >
                Apprenticeships and traineeships
              </h1>

              <p className="govuk-body-s govuk-!-margin-bottom-0">
                Published 01 October 2023
              </p>
              <p className="govuk-body-s">
                Last updated 31 October 2023,{' '}
                <a href="#releaseDetails" className="govuk-link--no-underline">
                  see all updates
                </a>
              </p>
            </div>
            <img
              src="/assets/images/UKSA-quality-mark.jpg"
              className="govuk-!-margin-right-3"
              alt="UK statistics authority quality mark"
              height="60"
              width="60"
            />
          </div>

          <div className="dfe-flex">
            <div>
              <div className={styles.prototypeConstrainWidth}>
                Apprenticeship and traineeship starts, achievements and
                participation. Includes breakdowns by age, sex, ethnicity,
                subject, provider, geography etc.
              </div>
            </div>
          </div>
        </>
      )}
      {sectionExample === 'header9' && (
        <>
          <span
            className="govuk-caption-xl dfe-flex dfe-align-items--center"
            id="topOfRelease"
          >
            <span className="govuk-tag govuk-!-margin-right-2">
              Latest data
            </span>{' '}
            Academic Year 2022/23
          </span>
          <h1
            className="govuk-heading-xl govuk-!-margin-bottom-2"
            data-testid="page-title"
          >
            Apprenticeships and traineeships
          </h1>

          <div className="dfe-flex dfe-justify-content--space-between govuk-!-margin-top-0">
            <div>
              <p className="govuk-body-s govuk-!-margin-bottom-3">
                Published 16 October 2023, next update October 2024
                <br />
                Last updated 31 October 2023,{' '}
                <a
                  href="#viewUpdates"
                  onClick={() => change && change()}
                  className="govuk-link--no-visited-stat"
                >
                  View all updates
                </a>
              </p>
              {/* 
               <a
                href="#"
                onClick={() => {
                  toggleReleaseTypeModal(true);
                }}
                className="govuk-body-s govuk-link--no-underline govuk-link--no-visited-state"
              >
                National statistics{' '}
                <InfoIcon description="What are national statistics?" />
              </a>{' '} */}
              <p className={classNames(styles.prototypeConstrainWidth)}>
                Apprenticeship and traineeship starts, achievements and
                participation. Includes breakdowns by age, sex, ethnicity,
                subject, provider, geography etc.
              </p>
            </div>

            <div
              className="dfe-flex dfe-align-items--center"
              style={{ flexDirection: 'column' }}
            >
              <a
                href="#"
                aria-label="What are national statistics?"
                onClick={() => change3 && change3()}
              >
                <img
                  src="/assets/images/UKSA-quality-mark.jpg"
                  className={styles.prototypeStatsTypeLogo}
                  alt="UK statistics authority quality mark"
                />
              </a>
            </div>
          </div>
          <Modal
            open={showReleaseTypeModal}
            title="What are National Statistics?"
            className="govuk-!-width-one-half"
          >
            <div>
              <p>
                These accredited official statistics have been independently
                reviewed by the{' '}
                <a href="https://osr.statisticsauthority.gov.uk/what-we-do/">
                  Office for Statistics Regulation
                </a>{' '}
                (OSR). They comply with the standards of trustworthiness,
                quality and value in the{' '}
                <a href="https://code.statisticsauthority.gov.uk/the-code/">
                  Code of Practice for Statistics
                </a>
                . Accredited official statistics are called National Statistics
                in the{' '}
                <a href="https://www.legislation.gov.uk/ukpga/2007/18/contents">
                  Statistics and Registration Service Act 2007
                </a>
                .
              </p>
              <p>
                Accreditation signifies their compliance with the authority's{' '}
                <a href="https://code.statisticsauthority.gov.uk/the-code/">
                  Code of Practice for Statistics
                </a>{' '}
                which broadly means these statistics are:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  managed impartially and objectively in the public interest
                </li>
                <li>meet identified user needs</li>
                <li>produced according to sound methods</li>
                <li>well explained and readily accessible</li>
              </ul>
              <p>
                Our statistical practice is regulated by the Office for
                Statistics Regulation (OSR).
              </p>
              <p>
                OSR sets the standards of trustworthiness, quality and value in
                the{' '}
                <a href="https://code.statisticsauthority.gov.uk/the-code/">
                  Code of Practice for Statistics
                </a>{' '}
                that all producers of official statistics should adhere to.
              </p>
              <p>
                You are welcome to contact us directly with any comments about
                how we meet these standards. Alternatively, you can contact OSR
                by emailing{' '}
                <a href="mailto:regulation@statistics.gov.uk">
                  regulation@statistics.gov.uk
                </a>{' '}
                or via the{' '}
                <a href="https://osr.statisticsauthority.gov.uk/">
                  OSR website
                </a>
                .
              </p>
            </div>
            <Button
              onClick={() => {
                toggleReleaseTypeModal(false);
              }}
            >
              Close
            </Button>
          </Modal>
        </>
      )}
      {sectionExample === 'quickLinks' && (
        <section className="govuk-!-margin-bottom-6">
          <h2 className="govuk-body">Quicklinks</h2>
          <ol
            className={classNames(styles.prototypeQuickLinks, 'govuk-list')}
            style={{ maxWidth: '100% !important' }}
          >
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">Download all data (ZIP)</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#releaseDetails">Methodologies</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">Data guidance</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">Pre release access list</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#releaseDetails">Other releases in this series</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#help">Help, support and contact</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">Sign up for email alerts</a>
                </span>
              </div>
            </li>
            <li>
              <div
                className={classNames(
                  styles.prototypeActionLink,
                  styles.prototypeDownloadLink,
                )}
              >
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#" className="govuk-link--no-underline">
                    <strong>Download all data (ZIP)</strong>
                  </a>
                </span>
              </div>
            </li>
          </ol>
        </section>
      )}
      {sectionExample === 'quickLinks2' && (
        <section className="govuk-!-margin-bottom-6">
          <h2 className="govuk-body govuk-visually-hidden">Quicklinks</h2>

          <ol
            className={classNames(styles.prototypeQuickLinks, 'govuk-list')}
            style={{ maxWidth: '100% !important' }}
          >
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapperDownload}>
                  <a href="#releaseDetails">Methodologies</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">Data guidance</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">Pre release access list</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#releaseDetails">Releases in this series (10)</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">View all updates (18)</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#help">Help, support and contact</a>
                </span>
              </div>
            </li>
            <li>
              <div className={styles.prototypeActionLink}>
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#">Sign up for email alerts</a>
                </span>
              </div>
            </li>
            <li>
              <div
                className={classNames(
                  styles.prototypeActionLink,
                  styles.prototypeDownloadLink,
                )}
              >
                <span className={styles.prototypeActionLinkWrapper}>
                  <a href="#" className="govuk-link--no-underline">
                    <strong>Download all data (ZIP)</strong>
                  </a>
                </span>
              </div>
            </li>
          </ol>
        </section>
      )}
      {sectionExample === 'summarySmall' && (
        <div className="dfe-content govuk-!-margin-bottom-9">
          <h4>October 2023 release</h4>
          <p>
            This transparency update adds additional data on monthly
            apprenticeship starts in the ‘Latest Apprenticeships in year data’
            section to cover the period August 2022 to July 2023 (based on data
            returned by providers in September 2023).
          </p>
          <h4>July 2023 release</h4>
          <p>
            This release shows provisional in-year data for apprenticeships and
            traineeships in England reported for the academic year 2022/23 to
            date (August 2022 to April 2023) based on data returned by providers
            in June 2023. This also includes apprenticeship service data (as of
            09 June 2023) and Find an apprenticeship data (to June 2023).
          </p>
          <h4>Changes to the structure of the release</h4>
          <p>
            In January we changed the structure of the release to improve user
            access to content and to allow for easier maintenance
            <strong>
              . The same amount of data is still being published on a quarterly
              basis.
            </strong>
            If you wish to provide feedback on these changes please contact us
            at{' '}
            <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
              FE.OFFICIALSTATISTICS@education.gov.uk
            </a>
            .
          </p>
          <p>
            As announced in November we are changing the content of the monthly
            updates in between the quarterly updates. Specifically, this
            includes February, April, May, June, August, September and October.
          </p>
          <p>
            We will continue to update the two existing apprenticeship starts
            files along with some narrative within the{' '}
            <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/2022-23#content-5-heading">
              latest apprenticeships in year data
            </a>{' '}
            accordion.
          </p>
          <p>
            All other data previously published monthly, such as that covering
            the apprenticeship service and find an apprenticeship, will be
            updated in the quarterly releases (January, March, July, and
            November).
          </p>
          <h4>
            <strong>
              Impact of COVID-19 on reporting of FE and apprenticeship data
            </strong>
          </h4>
          <p>
            Historic data in this release covers periods affected by varying
            COVID-19 restrictions, which impacted on apprenticeship and
            traineeship learning and also provider reporting behaviour via the
            Individualised Learner Record. Therefore, extra care should be taken
            in comparing and interpreting data presented in this release.
          </p>
          <p>
            <strong>Please note that the ‘</strong>
            <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
              <strong>Explore data and files used in this release</strong>
            </a>
            ’ section contains the underlying files that underpin this release
            and allows expert users to interrogate and analyse the data for
            themselves. For pre-populated summary statistics please see the
            relevant section underneath, from which the data can be further
            explored using the ‘Explore data’ functionality. You can also view
            featured tables or create your own table using the ‘
            <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
              <strong>create your own tables</strong>
            </a>
            <strong>' functionality.</strong>
          </p>
        </div>
      )}

      {sectionExample === 'summary' && (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-full">
            <h2 className="govuk-heading-l">
              Methodologies and release details
            </h2>
            <dl className="govuk-summary-list govuk-summary-list--no-border">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Published</dt>
                <dd className="govuk-summary-list__value">
                  <time>27 January 2022 </time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Release type</dt>
                <dd className="govuk-summary-list__value">
                  <a href="#exploreData">National data</a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Methodologies</dt>
                <dd className="govuk-summary-list__value">
                  <a href="/methodology/further-education-and-skills-statistics-methodology">
                    Further education and skills statistics: methodology
                  </a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Related information</dt>
                <dd className="govuk-summary-list__value">
                  <ul className="govuk-list">
                    <li>
                      <a href="#">Data guidance</a>
                    </li>
                    <li>
                      <a href="#">Pre-release access list</a>
                    </li>
                  </ul>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Last updated </dt>
                <dd className="govuk-summary-list__value">
                  <Details
                    className="govuk-!-margin-0"
                    summary="22 December 2022 (18 updates)"
                  >
                    <ol className="govuk-list">
                      <li>
                        <time className="govuk-body govuk-!-font-weight-bold">
                          18 December 2020
                        </time>
                        <p>
                          Local authority level data on placement stability and
                          local authority level data for care leavers have been
                          added.
                        </p>
                      </li>
                      <li>
                        <time className="govuk-body govuk-!-font-weight-bold">
                          11 December 2020
                        </time>
                        <p>
                          Correction to metadata for LA level 'children ceasing
                          to be looked after' file
                        </p>
                      </li>
                    </ol>
                  </Details>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">
                  Releases in this series{' '}
                </dt>
                <dd className="govuk-summary-list__value">
                  <Details
                    className="govuk-!-margin-0"
                    summary="View releases (8)"
                  >
                    <ol className="govuk-list">
                      <li>Test</li>
                    </ol>
                  </Details>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Receive updates </dt>
                <dd className="govuk-summary-list__value">
                  <a
                    data-testid="subscription-children-looked-after-in-england-including-adoptions"
                    href="/subscriptions?slug=children-looked-after-in-england-including-adoptions"
                    className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                  >
                    <strong>Sign up for email alerts</strong>
                  </a>
                </dd>
              </div>
            </dl>{' '}
            <hr />
            <div className="dfe-content">
              <h4>October 2023 release</h4>
              <p>
                This transparency update adds additional data on monthly
                apprenticeship starts in the ‘Latest Apprenticeships in year
                data’ section to cover the period August 2022 to July 2023
                (based on data returned by providers in September 2023).
              </p>
              <h4>July 2023 release</h4>
              <p>
                This release shows provisional in-year data for apprenticeships
                and traineeships in England reported for the academic year
                2022/23 to date (August 2022 to April 2023) based on data
                returned by providers in June 2023. This also includes
                apprenticeship service data (as of 09 June 2023) and Find an
                apprenticeship data (to June 2023).
              </p>
              <h4>Changes to the structure of the release</h4>
              <p>
                In January we changed the structure of the release to improve
                user access to content and to allow for easier maintenance
                <strong>
                  . The same amount of data is still being published on a
                  quarterly basis.
                </strong>
                If you wish to provide feedback on these changes please contact
                us at{' '}
                <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
                  FE.OFFICIALSTATISTICS@education.gov.uk
                </a>
                .
              </p>
              <p>
                As announced in November we are changing the content of the
                monthly updates in between the quarterly updates. Specifically,
                this includes February, April, May, June, August, September and
                October.
              </p>
              <p>
                We will continue to update the two existing apprenticeship
                starts files along with some narrative within the{' '}
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/2022-23#content-5-heading">
                  latest apprenticeships in year data
                </a>{' '}
                accordion.
              </p>
              <p>
                All other data previously published monthly, such as that
                covering the apprenticeship service and find an apprenticeship,
                will be updated in the quarterly releases (January, March, July,
                and November).
              </p>
              <h4>
                <strong>
                  Impact of COVID-19 on reporting of FE and apprenticeship data
                </strong>
              </h4>
              <p>
                Historic data in this release covers periods affected by varying
                COVID-19 restrictions, which impacted on apprenticeship and
                traineeship learning and also provider reporting behaviour via
                the Individualised Learner Record. Therefore, extra care should
                be taken in comparing and interpreting data presented in this
                release.
              </p>
              <p>
                <strong>Please note that the ‘</strong>
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                  <strong>Explore data and files used in this release</strong>
                </a>
                ’ section contains the underlying files that underpin this
                release and allows expert users to interrogate and analyse the
                data for themselves. For pre-populated summary statistics please
                see the relevant section underneath, from which the data can be
                further explored using the ‘Explore data’ functionality. You can
                also view featured tables or create your own table using the ‘
                <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
                  <strong>create your own tables</strong>
                </a>
                <strong>' functionality.</strong>
              </p>
            </div>
          </div>
        </div>
      )}

      {sectionExample === 'summaryFull' && (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-full">
            <h2 className="govuk-heading-l" id="releaseDetails">
              Release details
            </h2>
            <dl className="govuk-summary-list govuk-summary-list--no-border">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Published</dt>
                <dd className="govuk-summary-list__value">
                  <time>27 January 2022 </time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Release type</dt>
                <dd className="govuk-summary-list__value">
                  <a href="#exploreData">National data</a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Methodologies</dt>
                <dd className="govuk-summary-list__value">
                  <a href="/methodology/further-education-and-skills-statistics-methodology">
                    Further education and skills statistics: methodology
                  </a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Related information</dt>
                <dd className="govuk-summary-list__value">
                  <ul className="govuk-list">
                    <li>
                      <a href="#">Data guidance</a>
                    </li>
                    <li>
                      <a href="#">Pre-release access list</a>
                    </li>
                  </ul>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Last updated </dt>
                <dd className="govuk-summary-list__value">
                  <ol className="govuk-list" data-testid="all-updates">
                    <li>
                      <time
                        data-testid="update-on"
                        className="govuk-body govuk-!-font-weight-bold"
                      >
                        16 October 2023
                      </time>
                      <p data-testid="update-reason">
                        Correction to footnote on one table
                      </p>
                    </li>
                  </ol>
                  <Details
                    className="govuk-!-margin-0"
                    summary="Show all other updates (18)"
                  >
                    <ol className="govuk-list">
                      <li>
                        <time className="govuk-body govuk-!-font-weight-bold">
                          18 December 2020
                        </time>
                        <p>
                          Local authority level data on placement stability and
                          local authority level data for care leavers have been
                          added.
                        </p>
                      </li>
                      <li>
                        <time className="govuk-body govuk-!-font-weight-bold">
                          11 December 2020
                        </time>
                        <p>
                          Correction to metadata for LA level 'children ceasing
                          to be looked after' file
                        </p>
                      </li>
                    </ol>
                  </Details>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">
                  10 releases in this series{' '}
                </dt>
                <dd className="govuk-summary-list__value">
                  <ul className="govuk-list">
                    <li data-testid="other-release-item">
                      <a
                        href="/find-statistics/apprenticeships-and-traineeships/2021-22"
                        className="govuk-link"
                      >
                        Academic year 2021/22, Latest data
                      </a>
                    </li>
                    <li data-testid="other-release-item">
                      <a
                        href="/find-statistics/apprenticeships-and-traineeships/2020-21"
                        className="govuk-link"
                      >
                        Academic year 2020/21
                      </a>
                    </li>
                    <li data-testid="other-release-item">
                      <a
                        href="/find-statistics/apprenticeships-and-traineeships/2019-20"
                        className="govuk-link"
                      >
                        Academic year 2019/20
                      </a>
                    </li>
                  </ul>

                  <Details
                    className="govuk-!-margin-0"
                    summary="View all other releases (7)"
                  >
                    <ul className="govuk-list">
                      <li data-testid="other-release-item">
                        <a href="https://www.gov.uk/government/statistics/further-education-and-skills-november-2019">
                          November 2019
                        </a>
                      </li>
                      <li data-testid="other-release-item">
                        <a href="https://www.gov.uk/government/statistics/further-education-and-skills-november-2018">
                          November 2018
                        </a>
                      </li>
                      <li data-testid="other-release-item">
                        <a href="https://www.gov.uk/government/statistics/further-education-and-skills-november-2017">
                          November 2017
                        </a>
                      </li>
                      <li data-testid="other-release-item">
                        <a href="https://www.gov.uk/government/statistics/further-education-and-skills-november-2016">
                          November 2016
                        </a>
                      </li>
                      <li data-testid="other-release-item">
                        <a href="https://www.gov.uk/government/statistics/further-education-and-skills-statistical-first-release-november-2015">
                          November 2015
                        </a>
                      </li>
                    </ul>
                  </Details>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Receive updates </dt>
                <dd className="govuk-summary-list__value">
                  <a
                    data-testid="subscription-children-looked-after-in-england-including-adoptions"
                    href="/subscriptions?slug=children-looked-after-in-england-including-adoptions"
                    className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                  >
                    <strong>Sign up for email alerts</strong>
                  </a>
                </dd>
              </div>
            </dl>{' '}
            <hr />
            <div className="dfe-content">
              <h4>October 2023 release</h4>
              <p>
                This transparency update adds additional data on monthly
                apprenticeship starts in the ‘Latest Apprenticeships in year
                data’ section to cover the period August 2022 to July 2023
                (based on data returned by providers in September 2023).
              </p>
              <h4>July 2023 release</h4>
              <p>
                This release shows provisional in-year data for apprenticeships
                and traineeships in England reported for the academic year
                2022/23 to date (August 2022 to April 2023) based on data
                returned by providers in June 2023. This also includes
                apprenticeship service data (as of 09 June 2023) and Find an
                apprenticeship data (to June 2023).
              </p>
              <h4>Changes to the structure of the release</h4>
              <p>
                In January we changed the structure of the release to improve
                user access to content and to allow for easier maintenance
                <strong>
                  . The same amount of data is still being published on a
                  quarterly basis.
                </strong>
                If you wish to provide feedback on these changes please contact
                us at{' '}
                <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
                  FE.OFFICIALSTATISTICS@education.gov.uk
                </a>
                .
              </p>
              <p>
                As announced in November we are changing the content of the
                monthly updates in between the quarterly updates. Specifically,
                this includes February, April, May, June, August, September and
                October.
              </p>
              <p>
                We will continue to update the two existing apprenticeship
                starts files along with some narrative within the{' '}
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/2022-23#content-5-heading">
                  latest apprenticeships in year data
                </a>{' '}
                accordion.
              </p>
              <p>
                All other data previously published monthly, such as that
                covering the apprenticeship service and find an apprenticeship,
                will be updated in the quarterly releases (January, March, July,
                and November).
              </p>
              <h4>
                <strong>
                  Impact of COVID-19 on reporting of FE and apprenticeship data
                </strong>
              </h4>
              <p>
                Historic data in this release covers periods affected by varying
                COVID-19 restrictions, which impacted on apprenticeship and
                traineeship learning and also provider reporting behaviour via
                the Individualised Learner Record. Therefore, extra care should
                be taken in comparing and interpreting data presented in this
                release.
              </p>
              <p>
                <strong>Please note that the ‘</strong>
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                  <strong>Explore data and files used in this release</strong>
                </a>
                ’ section contains the underlying files that underpin this
                release and allows expert users to interrogate and analyse the
                data for themselves. For pre-populated summary statistics please
                see the relevant section underneath, from which the data can be
                further explored using the ‘Explore data’ functionality. You can
                also view featured tables or create your own table using the ‘
                <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
                  <strong>create your own tables</strong>
                </a>
                <strong>' functionality.</strong>
              </p>
            </div>
          </div>
        </div>
      )}

      {sectionExample === 'headlines' && (
        <>
          <h2 className="govuk-heading-l" id="headlines">
            Headline facts and figures
          </h2>

          <div className={styles.prototypeKeyStatsContainer}>
            <div className={styles.prototypeKeyStatIndividual}>
              <div className={stylesKeyStatTile.tile}>
                <h3 className="govuk-heading-s">Starts (Aug - Apr)</h3>
                <p className="govuk-heading-l">275,630</p>
                <p className="govuk-body-s">down by 4.6% from 2021/22</p>
              </div>
              <div className={styles.prototypeKeyStatHelp}>
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeKeyStatHelpLink,
                    'govuk-link--no-visited-state',
                    'govuk-link--no-underline',
                  )}
                  onClick={e => {
                    setShowHelp1(!showHelp1);
                    e.preventDefault();
                  }}
                >
                  <span>What is this</span>
                  <InfoIcon description="What is status?" />
                </a>
                {showHelp1 && (
                  <div className={styles.prototypeKeyStatHelpDialog}>
                    All-age (16+) apprenticeship starts in England for the
                    2022/23 academic year.
                    <p className="govuk-!-margin-top-6 govuk-body-s">
                      <a
                        href="#"
                        onClick={e => {
                          setShowHelp1(!showHelp1);
                          e.preventDefault();
                        }}
                      >
                        Close
                      </a>
                    </p>
                  </div>
                )}
              </div>
            </div>

            <div className={styles.prototypeKeyStatIndividual}>
              <div className={stylesKeyStatTile.tile}>
                <h3 className="govuk-heading-s">Participation (Aug - Apr)</h3>
                <p className="govuk-heading-l">703,670</p>
                <p className="govuk-body-s">up by 1.6% from 2021/22</p>
              </div>
              <div className={styles.prototypeKeyStatHelp}>
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeKeyStatHelpLink,
                    'govuk-link--no-visited-state',
                    'govuk-link--no-underline',
                  )}
                  onClick={e => {
                    setShowHelp2(!showHelp2);
                    e.preventDefault();
                  }}
                >
                  <span>What is this</span>
                  <InfoIcon description="What is status?" />
                </a>
                {showHelp2 && (
                  <div className={styles.prototypeKeyStatHelpDialog}>
                    All-age (16+) apprenticeship participation in England for
                    the 2022/23 academic year.
                    <p className="govuk-!-margin-top-6 govuk-body-s">
                      <a
                        href="#"
                        onClick={e => {
                          setShowHelp2(!showHelp2);
                          e.preventDefault();
                        }}
                      >
                        Close
                      </a>
                    </p>
                  </div>
                )}
              </div>
            </div>

            <div className={styles.prototypeKeyStatIndividual}>
              <div className={stylesKeyStatTile.tile}>
                <h3 className="govuk-heading-s">Achievements (Aug - Apr)</h3>
                <p className="govuk-heading-l">105,600</p>
                <p className="govuk-body-s">up by 20.1% from 2021/22</p>
              </div>
              <div className={styles.prototypeKeyStatHelp}>
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeKeyStatHelpLink,
                    'govuk-link--no-visited-state',
                    'govuk-link--no-underline',
                  )}
                  onClick={e => {
                    setShowHelp3(!showHelp3);
                    e.preventDefault();
                  }}
                >
                  <span>What is this</span>
                  <InfoIcon description="What is status?" />
                </a>
                {showHelp3 && (
                  <div className={styles.prototypeKeyStatHelpDialog}>
                    All-age (16+) apprenticeship achievements in England for the
                    2022/23 academic year.
                    <p className="govuk-!-margin-top-6 govuk-body-s">
                      <a
                        href="#"
                        onClick={e => {
                          setShowHelp3(!showHelp3);
                          e.preventDefault();
                        }}
                      >
                        Close
                      </a>
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>
          <div className="dfe-content">
            <h4>
              <strong>Figures for the 2022/23 academic year show:</strong>
            </h4>
            <ol
              className={classNames(
                styles.prototypeCardDashboardContainerGrid,
                'govuk-!-padding-0',
              )}
            >
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Apprenticeship starts were down by <em>4.6%</em> to 275,630
                  compared to 288,800 reported for the same period in the
                  previous year.
                </p>
              </li>
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Under 19s accounted for <em>24.8% </em>of starts (68,290).
                </p>
              </li>
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Advanced apprenticeships accounted for <em>43.2%</em> of
                  starts (119,170) whilst higher apprenticeships accounted for a{' '}
                  <em>34.0%</em>
                  of starts (93,970).
                </p>
              </li>
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Higher apprenticeships continue to grow in 2022/23. Higher
                  apprenticeship starts increased by <em>6.1%</em> to 93,670
                  compared to 88,240 in the same period last year.
                </p>
              </li>
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Starts at Level 6 and 7 increased by <em>9.3% </em>to 41,340
                  in 2022/23. This represents <em>15.0%</em> of all starts
                  reported to date for 2022/23.
                </p>
              </li>
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Starts supported by Apprenticeship Service Account (ASA) levy
                  funds accounted for <em>67.0% </em>(184,570).
                </p>
              </li>
              {/* 
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Since May 2015 there have been <em>3,157,480</em>{' '}
                  apprenticeship starts. Since May 2010 this total stands at
                  <em>5,535,020</em>.
                </p>
              </li>
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Apprenticeship achievements increased by <em>20.1%</em> to
                  105,600 compared to 87,920 reported for the same period in the
                  previous year.
                </p>
              </li>
              <li className={styles.prototypeDashboardCard}>
                <p>
                  Learner participation increased by <em>1.6%</em> to 703,670
                  compared to 692,920 reported for the same period in the
                  previous year.
                </p>
              </li>
              */}
            </ol>
            <p>
              Please note: COVID-19 restrictions and assessment flexibilities
              affected the timing of achievements, therefore care must be taken
              when comparing achievements between years as some achievements
              expected in a given academic year may have been delayed to the
              subsequent year.
            </p>
          </div>
        </>
      )}
      {sectionExample === 'headlines2' && (
        <>
          <h2 className="govuk-heading-l" id="headlines">
            Headline facts and figures
          </h2>

          <div className={styles.prototypeKeyStatsContainer}>
            <div className={styles.prototypeKeyStatIndividual}>
              <div className={stylesKeyStatTile.tile}>
                <h3 className="govuk-heading-s">Starts (Aug - Apr)</h3>
                <p className="govuk-heading-l">275,630</p>
                <p className="govuk-body-s">down by 4.6% from 2021/22</p>
              </div>
              <div className={styles.prototypeKeyStatHelp}>
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeKeyStatHelpLink,
                    'govuk-link--no-visited-state',
                    'govuk-link--no-underline',
                  )}
                  onClick={e => {
                    setShowHelp1(!showHelp1);
                    e.preventDefault();
                  }}
                >
                  <span>Apprenticeship starts</span>
                  <InfoIcon description="What are apprenticeship starts?" />
                </a>
                {showHelp1 && (
                  <div className={styles.prototypeKeyStatHelpDialog}>
                    All-age (16+) apprenticeship starts in England for the
                    2022/23 academic year.
                    <p className="govuk-!-margin-top-6 govuk-body-s">
                      <a
                        href="#"
                        onClick={e => {
                          setShowHelp1(!showHelp1);
                          e.preventDefault();
                        }}
                      >
                        Close
                      </a>
                    </p>
                  </div>
                )}
              </div>
            </div>

            <div className={styles.prototypeKeyStatIndividual}>
              <div className={stylesKeyStatTile.tile}>
                <h3 className="govuk-heading-s">Participation (Aug - Apr)</h3>
                <p className="govuk-heading-l">703,670</p>
                <p className="govuk-body-s">up by 1.6% from 2021/22</p>
              </div>
              <div className={styles.prototypeKeyStatHelp}>
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeKeyStatHelpLink,
                    'govuk-link--no-visited-state',
                    'govuk-link--no-underline',
                  )}
                  onClick={e => {
                    setShowHelp2(!showHelp2);
                    e.preventDefault();
                  }}
                >
                  <span>Apprenticeship participation</span>
                  <InfoIcon description="Apprenticeship starts?" />
                </a>
                {showHelp2 && (
                  <div className={styles.prototypeKeyStatHelpDialog}>
                    All-age (16+) apprenticeship participation in England for
                    the 2022/23 academic year.
                    <p className="govuk-!-margin-top-6 govuk-body-s">
                      <a
                        href="#"
                        onClick={e => {
                          setShowHelp2(!showHelp2);
                          e.preventDefault();
                        }}
                      >
                        Close
                      </a>
                    </p>
                  </div>
                )}
              </div>
            </div>

            <div className={styles.prototypeKeyStatIndividual}>
              <div className={stylesKeyStatTile.tile}>
                <h3 className="govuk-heading-s">Achievements (Aug - Apr)</h3>
                <p className="govuk-heading-l">105,600</p>
                <p className="govuk-body-s">up by 20.1% from 2021/22</p>
              </div>
              <div className={styles.prototypeKeyStatHelp}>
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeKeyStatHelpLink,
                    'govuk-link--no-visited-state',
                    'govuk-link--no-underline',
                  )}
                  onClick={e => {
                    setShowHelp3(!showHelp3);
                    e.preventDefault();
                  }}
                >
                  <span>Apprenticeships acheivements</span>
                  <InfoIcon description="What are apprenticeships acheivements?" />
                </a>
                {showHelp3 && (
                  <div className={styles.prototypeKeyStatHelpDialog}>
                    All-age (16+) apprenticeship achievements in England for the
                    2022/23 academic year.
                    <p className="govuk-!-margin-top-6 govuk-body-s">
                      <a
                        href="#"
                        onClick={e => {
                          setShowHelp3(!showHelp3);
                          e.preventDefault();
                        }}
                      >
                        Close
                      </a>
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>
          <div className="dfe-content">
            <h4>
              <strong>Figures for the 2022/23 academic year show:</strong>
            </h4>
            <ul className="govuk-list govuk-list--bullet govuk-list--spaced">
              <li>
                Apprenticeship starts were down by <strong>4.6%</strong> to
                275,630 compared to 288,800 reported for the same period in the
                previous year.
              </li>
              <li>
                Under 19s accounted for <strong>24.8% </strong>of starts
                (68,290).
              </li>
              <li>
                Advanced apprenticeships accounted for <strong>43.2%</strong> of
                starts (119,170) whilst higher apprenticeships accounted for a{' '}
                <strong>34.0%</strong>
                of starts (93,970).
              </li>
              <li>
                Higher apprenticeships continue to grow in 2022/23. Higher
                apprenticeship starts increased by <strong>6.1%</strong> to
                93,670 compared to 88,240 in the same period last year.
              </li>
              <li>
                Starts at Level 6 and 7 increased by <strong>9.3% </strong>to
                41,340 in 2022/23. This represents <strong>15.0%</strong> of all
                starts reported to date for 2022/23.
              </li>
              <li>
                Starts supported by Apprenticeship Service Account (ASA) levy
                funds accounted for <strong>67.0% </strong>(184,570).
              </li>

              <li>
                Since May 2015 there have been <strong>3,157,480</strong>{' '}
                apprenticeship starts. Since May 2010 this total stands at
                <strong>5,535,020</strong>.
              </li>
              <li>
                Apprenticeship achievements increased by <strong>20.1%</strong>{' '}
                to 105,600 compared to 87,920 reported for the same period in
                the previous year.
              </li>
              <li>
                Learner participation increased by <strong>1.6%</strong> to
                703,670 compared to 692,920 reported for the same period in the
                previous year.
              </li>
            </ul>
            <p>
              Please note: COVID-19 restrictions and assessment flexibilities
              affected the timing of achievements, therefore care must be taken
              when comparing achievements between years as some achievements
              expected in a given academic year may have been delayed to the
              subsequent year.
            </p>
          </div>
        </>
      )}
      {sectionExample === 'explore' && (
        <>
          <h2 className="govuk-heading-l govuk-!-margin-top-6" id="exploreData">
            Explore data and files used in this release
          </h2>
          <div
            className={classNames(
              styles.prototypeCardContainerGrid,
              styles.prototypeCardBg,
              'govuk-!-margin-bottom-9',
            )}
          >
            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="./table-highlights-2?source=publicationPage"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  View or create your own tables
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                View featured tables that we have built for you, or create your
                own tables from open data using our table tool
              </p>
            </div>

            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/data-catalog?theme=fe&publication=traineeships&source=publicationPage"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  Data catalogue
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Browse and download individual open data files from this release
                in our data catalogue
              </p>
            </div>

            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  Download all data (ZIP)
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Download all data available in this release as a compressed ZIP
                file
              </p>
            </div>
            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/data-guidance"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  Data guidance
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Learn more about the data files used in this release using our
                online guidance
              </p>
            </div>
          </div>
        </>
      )}
      {sectionExample === 'explore2' && (
        <>
          <h2 className="govuk-heading-l" id="exploreData">
            Explore data and files used in this release
          </h2>
          <div
            className={classNames(
              styles.prototypeCardBg,
              'govuk-!-margin-bottom-9',
            )}
          >
            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="#"
                  className={classNames('govuk-link--no-visited-state')}
                >
                  Download all data (ZIP)
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Download all data available in this release as a compressed ZIP
                file
              </p>
            </div>
            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="./table-highlights-2?source=publicationPage"
                  className={classNames('govuk-link--no-visited-state')}
                >
                  View or create your own tables
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                View featured tables that we have built for you, or create your
                own tables from open data using our table tool
              </p>
            </div>

            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/data-catalog?theme=fe&publication=traineeships&source=publicationPage"
                  className={classNames('govuk-link--no-visited-state')}
                >
                  Data catalogue
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Browse and download individual open data files from this release
                in our data catalogue
              </p>
            </div>
            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="#"
                  onClick={() => change2 && change2()}
                  className={classNames('govuk-link--no-visited-state')}
                >
                  Data guidance
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Learn more about the data files used in this release using our
                online guidance
              </p>
            </div>

            {/* 
            <div className={classNames(styles.prototypeCardChevron)}>
              <h2
                className={classNames(
                  'govuk-heading-s',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/data-guidance"
                  className={classNames('govuk-link--no-visited-state')}
                >
                  Data guidance
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Learn more about the data files used in this release using our
                online guidance
              </p>
            </div>
            */}
          </div>
        </>
      )}
      {sectionExample === 'about' && (
        <>
          <h2 id="about" className="govuk-!-margin-top-9">
            About these statistics
          </h2>
          <h3>November 2023 release</h3>
          <p>
            This release shows full-year data on apprenticeships and
            traineeships in England for the 2022/23 academic year covering the
            period August 2022 to July 2023 (based on data returned by providers
            in October 2023).
          </p>
          <p>
            This update also includes the latest available apprenticeship
            service data (as of 30 October 2023) and Find an apprenticeship data
            (to October 2023).
          </p>
          <p>
            This statistical release presents provisional information on all age
            (16+) apprenticeships starts, achievements and participation in
            England for the 2022/23 academic year.
          </p>
          <p>Also published are official statistics covering:</p>
          <ul>
            <li>Apprenticeship service commitments</li>
            <li>
              Employers reporting the withdrawal of apprentices due to
              redundancy
            </li>
            <li>
              Adverts and vacancies as reported on the Find an apprenticeship
              website
            </li>
          </ul>
          <p>
            A separate release covers overall further education and skills data,
            please see ‘
            <a href="https://explore-education-statistics.service.gov.uk/find-statistics/further-education-and-skills">
              Further education and skills
            </a>
            ’. Please note that the FE and skills release includes the adult
            apprenticeships and traineeships published here in its headline
            figures.
          </p>

          <h3>Changes to the structure of the release</h3>
          <p>
            In January we changed the structure of the release to improve user
            access to content and to allow for easier maintenance
            <strong>
              . The same amount of data is still being published on a quarterly
              basis.&nbsp;
            </strong>
            If you wish to provide feedback on these changes please contact us
            at{' '}
            <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
              FE.OFFICIALSTATISTICS@education.gov.uk
            </a>
            .
          </p>
          <p>
            As announced in November 2022 we are changing the content of the
            monthly updates in between the quarterly updates.&nbsp;Specifically,
            this includes February, April, May, June, August, September and
            October.
          </p>
          <p>
            All other data previously published monthly, such as that covering
            the apprenticeship service and find an apprenticeship, will be
            updated in the quarterly releases (January, March, July, and
            November).
          </p>
          <h3>Individualised Learner Record (ILR) administrative data</h3>
          <p>
            The apprenticeship data in this release published in July 2023 are
            based on the tenth ILR data return from FE and apprenticeship
            providers for the 2022/23 academic year, which was taken in June
            2023. The October monthly transparency update is based on the
            thirteenth ILR data return (taken in September 2023). The ILR is an
            administrative data collection system designed primarily for
            operational use in order to fund training providers for learners in
            FE and on apprenticeship programmes.
          </p>
          <h3>National achievement rate tables data</h3>
          <p>
            Figures in the ‘national achievement rate tables’ section are as
            published in March 2023. These official statistics cover achievement
            rates for apprenticeships in the 2021/22 academic year and would
            have been previously released as part of the standalone National
            achievement rate tables publication.
          </p>
          <h3>
            <strong>Provider reporting during the COVID-19 pandemic</strong>
          </h3>
          <p>
            Historic data in this publication covers periods affected by varying
            COVID-19 restrictions, which will have impacted on apprenticeship
            and traineeship learning. Therefore, extra care should be taken in
            comparing and interpreting data presented in this release.
          </p>
          <p>
            The furlough scheme may also have impacted on how aspects of ILR
            data were recorded, such as how the ‘learning status’ of a learner
            was captured, e.g. whether a learner was recorded as a continuing
            learner or whether they were recorded as being on a break in
            learning while still being with an employer.
          </p>
        </>
      )}
      {sectionExample === 'supplement' && (
        <>
          <h2 id="supplement" className="govuk-!-margin-top-9">
            How to find data and supplementary tables in this release
          </h2>
          <p>
            The Apprenticeships and traineeships publication still provides the
            same range of data it always did, but has undergone some structural
            changes since the previous publication in order to improve user’s
            experience.
          </p>
          <p>
            We have also adopted a new naming convention for files to help users
            find their data of interest. We have not changed the content of
            these files except in a few cases where we have merged some smaller
            files. You can find a look-up of the old and new file names in the
            file called
            <strong> “New Release Layout - Names Lookup” </strong>that can be
            found by clicking '
            <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
              Explore data and files
            </a>
            ' and opening the ‘all supporting files’ section.
          </p>
          <p>
            <strong>
              This section serves to signpost users to the data most relevant to
              their uses by detailing the routes through which it can be
              accessed.
            </strong>
          </p>
          <p>
            <strong>
              The content of the publication below contains charts and tables
              which highlight key figures
            </strong>{' '}
            and trends that give an overview of the national picture of the
            apprenticeship and traineeship landscape.
          </p>
          <p>
            <strong>
              'Featured tables' provide further detail with figures broken down
              by common areas of interest.{' '}
            </strong>
            These can be found by expanding the '
            <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
              Explore data and files
            </a>
            ' accordion and clicking '
            <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
              Create tables
            </a>
            '. These tables are created to provide the next level of detail one
            might wish to find below the level of detail provided by tables
            embedded within the release. They also provide the user the
            opportunity to then amend content, reorder and take away to meet
            their needs. Within the release we list out the most relevant
            featured tables at the end of each commentary section.
          </p>
          <p>
            <strong>
              In addition to featured tables you can also access underlying data
              files
            </strong>
            and build your own tables using the table builder tool. For example,
            the featured table showing enrolments by provider is produced from
            an underlying data file which also contains detail on the level of
            an aim, and it's sector subject area.
          </p>
          <p>
            The list of files available can be accessed by expanding the '
            <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
              Explore data and files
            </a>
            ' accordion and clicking either ‘browse data files’ or '
            <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
              Create tables
            </a>
            ', then switching to the 'Create your own table' tab and selecting
            your file of interest.
          </p>
          <p>
            Alternatively you can modify and existing featured table by
            selecting it and then depending on the breakdowns available, edit
            the location, time periods, indicators and/or filters (Steps 3, 4
            and 5).
          </p>
          <p>
            <strong>There is a dashboard </strong>that provides interactive
            presentation of our published data, with a number of different views
            on to data and ‘drilldown’ capability to allow users to investigate
            different types of FE provision. It is particularly helpful in
            viewing data across different geographical areas and providers.
          </p>
          <p>
            <strong>This release also provides ‘all supporting files’</strong>
            which can be found at the end of the '
            <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
              Explore data and files
            </a>
            ' accordion.<strong> </strong>These are mainly csv files which can
            be downloaded, and provide some additional breakdowns including
            unrounded data. They are provided for transparency to enable
            analysts to re-use the data in this release. A metadata document is
            available in the same location which explains the content of these
            supporting files.
          </p>
          <p>
            All of the data available in this release can be downloaded using
            the 'Download all data (zip)' button at the top right of this page.
          </p>
          <p>
            <strong>Feedback</strong>
          </p>
          <p>
            This release is a structural change to how we publish our data and
            statistics, which we continually look to improve. As a result, your
            feedback is important to help us further improve and develop. To
            provide feedback on this release, please email us at
            <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
              FE.OFFICIALSTATISTICS@education.gov.uk
            </a>
            .
          </p>
        </>
      )}
      {sectionExample === 'fullYearApprenticeship' && (
        <>
          <h2 id="fullYearApprenticeships" className="govuk-!-margin-top-9">
            Full year apprenticeship landscape
          </h2>
          <blockquote>
            <p>
              The figures in this section relate to full-year final data up to
              and including the 2021/22 academic year and were originally
              published in November 2022
            </p>
          </blockquote>

          <h3>The changing apprenticeship landscape</h3>
          <p>
            Reform of the apprenticeships programme, along with the impact of
            the COVID-19 pandemic have influenced the trends presented in this
            section. Three main factors are set out in the graphic below.
          </p>

          <PrototypeChartExamples chartType="barChart" />
          <PrototypeChartExamples chartType="map" />
          <PrototypeChartExamples chartType="lineChart" />
        </>
      )}
      {sectionExample === 'fullYearTraineeship' && (
        <>
          <h2 id="fullYearTraineeships" className="govuk-!-margin-top-9">
            Full year traineeships
          </h2>
          <blockquote>
            <p>
              The figures in this section relate to full-year final data up to
              and including the 2021/22 academic year and were originally
              published in November 2022
            </p>
          </blockquote>

          <h3>The changing apprenticeship landscape</h3>
          <p>
            Reform of the apprenticeships programme, along with the impact of
            the COVID-19 pandemic have influenced the trends presented in this
            section. Three main factors are set out in the graphic below.
          </p>

          <PrototypeChartExamples chartType="barChart" />
          <PrototypeChartExamples chartType="map" />
          <PrototypeChartExamples chartType="lineChart" />
        </>
      )}
      {sectionExample === 'latestApprenticeship' && (
        <>
          <h2 id="latestApprenticeships" className="govuk-!-margin-top-9">
            Latest Apprenticeships in year data
          </h2>
          <blockquote>
            <p>
              The figures in this section relate to full-year final data up to
              and including the 2021/22 academic year and were originally
              published in November 2022
            </p>
          </blockquote>

          <h3>The changing apprenticeship landscape</h3>
          <p>
            Reform of the apprenticeships programme, along with the impact of
            the COVID-19 pandemic have influenced the trends presented in this
            section. Three main factors are set out in the graphic below.
          </p>

          <PrototypeChartExamples chartType="barChart" />
          <PrototypeChartExamples chartType="map" />
          <PrototypeChartExamples chartType="lineChart" />
        </>
      )}
      {sectionExample === 'latestTraineeship' && (
        <>
          <h2 id="latestTraineeships" className="govuk-!-margin-top-9">
            Latest Traineeships in year data
          </h2>
          <blockquote>
            <p>
              The figures in this section relate to full-year final data up to
              and including the 2021/22 academic year and were originally
              published in November 2022
            </p>
          </blockquote>

          <h3>The changing apprenticeship landscape</h3>
          <p>
            Reform of the apprenticeships programme, along with the impact of
            the COVID-19 pandemic have influenced the trends presented in this
            section. Three main factors are set out in the graphic below.
          </p>

          <PrototypeChartExamples chartType="barChart" />
          <PrototypeChartExamples chartType="map" />
          <PrototypeChartExamples chartType="lineChart" />
        </>
      )}
      {sectionExample === 'interactiveTool' && (
        <>
          <h2 id="interactiveTool" className="govuk-!-margin-top-9">
            Interactive data visualisation tool
          </h2>
          <div className="dfe-content">
            <p>
              The ;
              <a
                href="https://app.powerbi.com/view?r=eyJrIjoiMWIwMjIwZGUtMjg4NS00Zjk1LWIxMmMtNjczODQzNDliZDM2IiwidCI6ImZhZDI3N2M5LWM2MGEtNGRhMS1iNWYzLWIzYjhiMzRhODJmOSIsImMiOjh9"
                target="_blank"
                rel="noopener noreferrer"
              >
                interactive data visualisation tool ;
              </a>
              has been developed to complement the Apprenticeships and
              traineeships publication. ;
            </p>
            <p>
              The tool provides a visual, interactive presentation of the data
              and gives users the capacity to investigate apprenticeship and
              traineeship provision across geographical areas and providers.
            </p>
          </div>
        </>
      )}
      {sectionExample === 'pubSectorApprenticeship' && (
        <>
          <h2 id="pubSectorApprenticeship" className="govuk-!-margin-top-9">
            Public sector apprenticeships 2022-23
          </h2>
          <div className="dfe-content">
            <blockquote>
              <p>
                The following statistics are classified as official statistics
                and have been produced in compliance with the Code of Practice
                for Statistics. They are not designated as national statistics
                by the United Kingdom Statistics Authority.
              </p>
            </blockquote>
            <h3>
              <strong>The public sector apprenticeship target</strong>
            </h3>
            <p>
              Between 1 April 2017 and 31 March 2022, public sector bodies in
              England with 250 or more staff were set a Government target to
              employ an average of at least 2.3% of their staff as new
              apprentice starts.
            </p>
            <p>
              Outcomes against the target were published in the
              <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/2021-22">
                Apprenticeships and traineeships 2021/22 release
              </a>
              and detailed information about measurement and the organisations
              in scope are set out in the
              <a href="https://explore-education-statistics.service.gov.uk/methodology/further-education-and-skills-statistics-methodology">
                Further education and skills statistics: methodology
              </a>
              .
            </p>
            <h3>
              <strong>Public sector apprenticeships data return 2022-23</strong>
            </h3>
            <p>
              Though no longer a statutory requirement, public sector bodies
              were asked to continue to collect and report data on their
              apprenticeships activity in 2022-23 to support transparency and
              external accountability, and to help maintain the momentum public
              sector bodies have built up.
            </p>
            <p>
              Full details relating to the revised guidance can be found in
              <a href="https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/1154343/_Publishing_public_sector_apprenticeships_data_May_2023.pdf">
                Publishing public sector apprenticeships data: guidance for
                public sector bodies, April 2023
              </a>
              .
            </p>
            <p>
              Figures in this section provide an early, condensed view of
              apprenticeships activity across public sector organisations that
              have made a 2022-23 data return. They focus on new apprenticeship
              starts as a proportion of all employees which is consistent with
              the measurement of the previous target.
            </p>
            <p>
              Additional data showing the change in prevalence of apprentices in
              the public sector workforce between the start and end of 2022-23
              will be published as part of final full-year Apprenticeships and
              traineeships release in November.
            </p>
          </div>
        </>
      )}
      {sectionExample === 'additionalData' && (
        <>
          <h2 id="addtionalData" className="govuk-!-margin-top-9">
            Additional analysis and transparency data
          </h2>
          <div className="dfe-content">
            <blockquote>
              <p>
                The following statistics are classified as official statistics
                and have been produced in line with the Code of Practice for
                Statistics, rather than being classed as national statistics and
                approved as such by the United Kingdom Statistics Authority. The
                statistics are included for transparency purposes.
              </p>
            </blockquote>
          </div>
          <div className="dfe-content">
            <PrototypeDashboardContent />
          </div>
        </>
      )}

      {sectionExample === 'help' && (
        <>
          <h2 className="govuk-heading-l" id="help">
            Help and support
          </h2>
          <h3 className="govuk-heading-m" id="methodology">
            Methodology
          </h3>
          <p>
            Find out how and why we collect, process and publish these
            statistics.
          </p>
          <p>
            <a
              href="/methodology/further-education-and-skills-statistics-methodology"
              className="govuk-link"
            >
              Further education and skills statistics: methodology
            </a>
          </p>

          <h3 id="contactUs">Contact us</h3>
          <p>
            If you have a specific enquiry about Apprenticeships and
            traineeships statistics and data:
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            FE Stats Dissemination
          </h4>
          <p className="govuk-body govuk-!-margin-top-0">
            Email:{' '}
            <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
              FE.OFFICIALSTATISTICS@education.gov.uk
            </a>
            <br />
            Contact name: Matthew Rolfe
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h4>
          <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
          <p>Telephone: 020 7783 8300</p>
          <p>
            Opening times: <br />
            Monday to Friday from 9.30am to 5pm (excluding bank holidays)
          </p>
        </>
      )}

      {sectionExample === 'otherReleases' && (
        <>
          <h2 id="releasesInSeries">Releases in this series</h2>
          <table className="govuk-!-margin-bottom-9">
            <caption className="govuk-visually-hidden">
              <h3>Releases in this series</h3>
            </caption>
            <thead>
              <tr>
                <th style={{ width: '60%' }}>Release period</th>
                <th style={{ width: '40%' }}>Publish date</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>
                  Academic year 2023/24
                  <span className="govuk-tag govuk-tag--grey govuk-!-margin-left-3">
                    Next update
                  </span>
                </td>
                <td>October 2024</td>
              </tr>
              <tr>
                <td>
                  <a href="#">Academic year 2022/23</a>{' '}
                  <span className="govuk-tag govuk-!-margin-left-3">
                    Latest data
                  </span>
                </td>
                <td>16 October 2023</td>
              </tr>
              <tr>
                <td>
                  <a href="#">Academic year 2020/21</a>
                </td>
                <td>21 January 2021</td>
              </tr>
              <tr>
                <td>
                  <a href="#">Academic year 2021/22</a>{' '}
                </td>
                <td>27 January 2022</td>
              </tr>
              <tr>
                <td>
                  <a href="#">Academic year 2020/21</a>
                </td>
                <td>21 January 2021</td>
              </tr>
              <tr>
                <td>
                  <a href="#">Academic year 2019/20</a>
                </td>
                <td>26 November 2020</td>
              </tr>
              <tr>
                <td>
                  <a href="#">November 2019</a>
                </td>
                <td>28 November 2019</td>
              </tr>
              <tr>
                <td>
                  <a href="#">November 2018</a>
                </td>
                <td>6 December 2018</td>
              </tr>
              <tr>
                <td>
                  <a href="#">November 2017</a>
                </td>
                <td>23 November 2017</td>
              </tr>
              <tr>
                <td>
                  <a href="#">November 2016</a>
                </td>
                <td>21 November 2017</td>
              </tr>
              <tr>
                <td>
                  <a href="#">November 2015</a>
                </td>
                <td>29 November 2017</td>
              </tr>
            </tbody>
          </table>
        </>
      )}

      {sectionExample === 'allUpdates' && (
        <div style={{ marginBottom: '300px' }}>
          <h2 id="viewUpdates">View all updates to this release</h2>
          <p>Academic year 2022/23</p>
          <ol className="govuk-list" data-testid="all-updates">
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                16 October 2023
              </time>
              <p data-testid="update-reason">
                Correction to footnote on one table
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                12 October 2023
              </time>
              <p data-testid="update-reason">
                Updated with monthly starts for provisional 2022/23 full
                academic year
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                7 September 2023
              </time>
              <p data-testid="update-reason">
                Updated with monthly starts for the first eleven months of
                2022/23
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                10 August 2023
              </time>
              <p data-testid="update-reason">
                Updated with monthly starts for the first ten months of 2022/23
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                20 July 2023
              </time>
              <p data-testid="update-reason">
                Updated to add links to the interactive data visualisation tool
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                20 July 2023
              </time>
              <p data-testid="update-reason">
                Updated with data covering August 2022 to April 2023
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                8 June 2023
              </time>
              <p data-testid="update-reason">
                Updated with the monthly starts for the first eight months of
                2022/23
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                4 May 2023
              </time>
              <p data-testid="update-reason">
                Updated with the monthly starts for the first seven months of
                2022/23.
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                13 April 2023
              </time>
              <p data-testid="update-reason">
                Updated with the latest monthly starts for the first six months
                of 2022/23. Added achievement rate supporting file containing
                provider types
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                30 March 2023
              </time>
              <p data-testid="update-reason">
                Updated to add links to the interactive data visualisation tool
              </p>
            </li>
            <li>
              <time
                data-testid="update-on"
                className="govuk-body govuk-!-font-weight-bold"
              >
                30 March 2023
              </time>
              <p data-testid="update-reason">
                Updated with data covering the first two quarters of 2022/23.
                Achievement rate data also added covering 2021/22{' '}
              </p>
            </li>
          </ol>
        </div>
      )}
      {sectionExample === 'methodologies' && (
        <>
          <h2>Methodologies</h2>
          <p>
            Find out how and why we collect, process and publish these
            statistics.
          </p>
          <ul className="govuk-list govuk-list--spaced">
            <li>
              <a
                href="/methodology/further-education-and-skills-statistics-methodology"
                className="govuk-link"
              >
                Further education and skills statistics: methodology
              </a>
            </li>
          </ul>
        </>
      )}
      {sectionExample === 'nationalStats' && (
        <>
          <h2>What are National Statistics?</h2>
          <p>
            These accredited official statistics have been independently
            reviewed by the{' '}
            <a href="https://osr.statisticsauthority.gov.uk/what-we-do/">
              Office for Statistics Regulation
            </a>{' '}
            (OSR). They comply with the standards of trustworthiness, quality
            and value in the{' '}
            <a href="https://code.statisticsauthority.gov.uk/the-code/">
              Code of Practice for Statistics
            </a>
            . Accredited official statistics are called National Statistics in
            the{' '}
            <a href="https://www.legislation.gov.uk/ukpga/2007/18/contents">
              Statistics and Registration Service Act 2007
            </a>
            .
          </p>
          <p>
            Accreditation signifies their compliance with the authority's{' '}
            <a href="https://code.statisticsauthority.gov.uk/the-code/">
              Code of Practice for Statistics
            </a>{' '}
            which broadly means these statistics are:
          </p>
          <ul className="govuk-list govuk-list--bullet">
            <li>managed impartially and objectively in the public interest</li>
            <li>meet identified user needs</li>
            <li>produced according to sound methods</li>
            <li>well explained and readily accessible</li>
          </ul>
          <p>
            Our statistical practice is regulated by the Office for Statistics
            Regulation (OSR).
          </p>
          <p>
            OSR sets the standards of trustworthiness, quality and value in the{' '}
            <a href="https://code.statisticsauthority.gov.uk/the-code/">
              Code of Practice for Statistics
            </a>{' '}
            that all producers of official statistics should adhere to.
          </p>
          <p>
            You are welcome to contact us directly with any comments about how
            we meet these standards. Alternatively, you can contact OSR by
            emailing{' '}
            <a href="mailto:regulation@statistics.gov.uk">
              regulation@statistics.gov.uk
            </a>{' '}
            or via the{' '}
            <a href="https://osr.statisticsauthority.gov.uk/">OSR website</a>.
          </p>
        </>
      )}
      {sectionExample === 'preRelease' && (
        <>
          <hr className={styles.prototypeSectionBreak} />
          <h2 className="govuk-!-margin-top-9" id="preRelease">
            Pre-release access list
          </h2>
          <p>Published 16 October 2023</p>
          <p>
            In the 12 October update to this release (Apprenticeships and
            traineeships: October 2023), besides Department for Education (DfE)
            professional and production staff, the following post holders were
            given pre-release access up to 24 hours before release.
          </p>
          <ul>
            <li>Secretary of State for Education</li>
            <li>Minister for Skills, Apprenticeships and Higher Education</li>
            <li>
              Parliamentary Under Secretary of State (Minister for the School
              System and Student Finance)
            </li>
            <li>Special Advisers (2 recipients)</li>
            <li>Permanent Secretary</li>
            <li>Director General, Skills</li>
            <li>Director of Apprenticeships and Skills Bootcamps</li>
            <li>Deputy Director, Skills Policy Analysis&nbsp;</li>
            <li>Deputy Director, Data Insight and Statistics</li>
            <li>Deputy Head of Profession (HoP)</li>
            <li>Programme Deputy Director, Apprenticeships&nbsp;</li>
            <li>Acting Head of Digital Engagement and Creative Content</li>
            <li>Senior Media Officer&nbsp;</li>
            <li>Media Officer</li>
            <li>Head of Apprenticeship Parliamentary team&nbsp;</li>
            <li>Apprenticeship Parliamentary Team Officer</li>
            <li>Funding Policy Manager</li>
            <li>Apprenticeship Funding Analyst</li>
          </ul>
        </>
      )}
      {sectionExample === 'guidance' && (
        <>
          <h2>Data guidance</h2>
          <p className="govuk-!-margin-bottom-8" data-testid="published-date">
            <strong>
              Published <time>16 October 2023</time>
            </strong>
          </p>
          <div className="dfe-content" data-testid="dataGuidance-content">
            <h3>Description</h3>
            <p>
              This document describes the contents of the underlying data files
              accompanying the ‘Apprenticeships and traineeships’ statistics
              publication. There is a{' '}
              <a href="https://explore-education-statistics.service.gov.uk/methodology/further-education-and-skills-statistics-methodology">
                Methodology document
              </a>{' '}
              that is linked under ‘Useful information’ at the top of the
              release, that contains further information on the statistics
              published here.
            </p>
            <h3>Coverage</h3>
            <p>England</p>
          </div>
          <h3 className="govuk-!-margin-top-6">Data files</h3>
          <p>
            All data files associated with this releases are listed below with
            guidance on their content. To download any of these files, please
            visit our{' '}
            <a
              href="/data-catalogue/apprenticeships-and-traineeships/2022-23"
              className="govuk-link"
            >
              data catalogue
            </a>
            .
          </p>
          <Accordion id="guidance">
            <AccordionSection heading="Achievement Rates Learner Characteristics - Volumes and Rates by Level, Age, Sex, LLDD, Ethnicity">
              <dl className="govuk-summary-list govuk-!-margin-bottom-6">
                <div className="govuk-summary-list__row" data-testid="Filename">
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Filename-key"
                  >
                    Filename
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Filename-value"
                  >
                    apps_narts_learner_detailed.csv
                  </dd>
                </div>
                <div
                  className="govuk-summary-list__row"
                  data-testid="Geographic levels"
                >
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Geographic levels-key"
                  >
                    Geographic levels
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Geographic levels-value"
                  >
                    National
                  </dd>
                </div>
                <div
                  className="govuk-summary-list__row"
                  data-testid="Time period"
                >
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Time period-key"
                  >
                    Time period
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Time period-value"
                  >
                    2019/20 to 2021/22
                  </dd>
                </div>
                <div className="govuk-summary-list__row" data-testid="Content">
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Content-key"
                  >
                    Content
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Content-value"
                  >
                    <div
                      className="dfe-content"
                      data-testid="fileGuidanceContent"
                    >
                      <p>Apprenticeship national achievement rate tables</p>
                      <p>
                        <strong>Academic year:</strong> 2019/20 to 2021/22
                      </p>
                      <p>
                        <strong>Indicators:</strong> Leavers, Completers,
                        Achievers, Pass rate, Retention rate, Achievement rate
                      </p>
                      <p>
                        <strong>Filters: </strong>Level, Age, Sex, LLDD,
                        Ethnicity Major&nbsp;
                      </p>
                    </div>
                  </dd>
                </div>
              </dl>
            </AccordionSection>
            <AccordionSection heading="Achievement Rates Learner Characteristics - Volumes and Rates by Std-fwk flag, STEM, SSA T1, Level, Detailed Level, Age, IMD quintile">
              <dl className="govuk-summary-list govuk-!-margin-bottom-6">
                <div className="govuk-summary-list__row" data-testid="Filename">
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Filename-key"
                  >
                    Filename
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Filename-value"
                  >
                    apps_narts_learner_detailed.csv
                  </dd>
                </div>
                <div
                  className="govuk-summary-list__row"
                  data-testid="Geographic levels"
                >
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Geographic levels-key"
                  >
                    Geographic levels
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Geographic levels-value"
                  >
                    National
                  </dd>
                </div>
                <div
                  className="govuk-summary-list__row"
                  data-testid="Time period"
                >
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Time period-key"
                  >
                    Time period
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Time period-value"
                  >
                    2019/20 to 2021/22
                  </dd>
                </div>
                <div className="govuk-summary-list__row" data-testid="Content">
                  <dt
                    className="govuk-summary-list__key SummaryList_key__Id7g7"
                    data-testid="Content-key"
                  >
                    Content
                  </dt>
                  <dd
                    className="govuk-summary-list__value"
                    data-testid="Content-value"
                  >
                    <div
                      className="dfe-content"
                      data-testid="fileGuidanceContent"
                    >
                      <p>Apprenticeship national achievement rate tables</p>
                      <p>
                        <strong>Academic year:</strong> 2019/20 to 2021/22
                      </p>
                      <p>
                        <strong>Indicators:</strong> Leavers, Completers,
                        Achievers, Pass rate, Retention rate, Achievement rate
                      </p>
                      <p>
                        <strong>Filters: </strong>Level, Age, Sex, LLDD,
                        Ethnicity Major&nbsp;
                      </p>
                    </div>
                  </dd>
                </div>
              </dl>
            </AccordionSection>
          </Accordion>
        </>
      )}
    </>
  );
};

export default ExampleSection;
