import classNames from 'classnames';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import PrototypeSectionExamples from '@admin/prototypes/components/PrototypeSectionExamples';
import React, { useState, useEffect, useRef } from 'react';
import { useMobileMedia } from '@common/hooks/useMedia';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import styles from './PrototypePublicPage.module.scss';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const getDimensions = (e: any) => {
  const { height } = e.getBoundingClientRect();
  /* eslint-disable-next-line */
  const offsetTop = e.offsetTop;
  const offsetBottom = offsetTop + height;

  return {
    height,
    offsetTop,
    offsetBottom,
  };
};

const PrototypeReleaseData = () => {
  const { isMedia: isMobileMedia } = useMobileMedia();
  const [showContents, setShowContents] = useState(!isMobileMedia);

  const [visibleSection, setVisibleSection] = useState<string>();

  const headerRef = useRef(null);
  const summaryRef = useRef(null);
  const headlinesRef = useRef(null);
  const exploreRef = useRef(null);
  const aboutRef = useRef(null);
  const supplementRef = useRef(null);
  const fullYearApprenticeshipRef = useRef(null);
  const fullYearTraineeshiphRef = useRef(null);
  const latestApprenticeshipRef = useRef(null);
  const latestTraineeshipRef = useRef(null);
  const interactiveToolRef = useRef(null);
  const pubSectorApprenticeshipRef = useRef(null);
  const addtionalDataRef = useRef(null);
  const helpRef = useRef(null);

  const sectionRefs = [
    { section: 'Summary', ref: summaryRef },
    { section: 'Headlines', ref: headlinesRef },
    { section: 'ExploreData', ref: exploreRef },
    { section: 'About', ref: aboutRef },
    { section: 'Supplement', ref: supplementRef },
    { section: 'FullYearApprenticeship', ref: fullYearApprenticeshipRef },
    { section: 'FullYearTraineeship', ref: fullYearTraineeshiphRef },
    { section: 'LatestApprenticeship', ref: latestApprenticeshipRef },
    { section: 'LatestTraineeship', ref: latestTraineeshipRef },
    { section: 'InteractiveTool', ref: interactiveToolRef },
    { section: 'PubSectorApprenticeship', ref: pubSectorApprenticeshipRef },
    { section: 'AdditionalData', ref: addtionalDataRef },
    { section: 'Help', ref: helpRef },
  ];

  useEffect(() => {
    const handleScroll = () => {
      const { height: headerHeight } = getDimensions(headerRef.current);
      const scrollPosition = window.scrollY + headerHeight;
      /* eslint-disable-next-line */
      const selected = sectionRefs.find(({ ref }) => {
        const ele = ref.current;
        if (ele) {
          const { offsetBottom, offsetTop } = getDimensions(ele);
          return scrollPosition > offsetTop && scrollPosition < offsetBottom;
        }
      });

      if (selected && selected.section !== visibleSection) {
        setVisibleSection(selected.section);
      } else if (!selected && visibleSection) {
        setVisibleSection(undefined);
      }
    };

    handleScroll();
    window.addEventListener('scroll', handleScroll);
    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [visibleSection]);

  return (
    <div className={classNames(styles.prototypePublicPage)}>
      <PrototypePage
        breadcrumbs={[
          {
            name: 'Further education',
            link: '#',
          },
          {
            name: 'Apprenticeships and traineeships',
            link: '#',
          },
        ]}
        wide={false}
      >
        <div ref={headerRef}>
          <PrototypeSectionExamples sectionExample="header8" />
          <div className="govuk-!-margin-top-6">
            <PrototypeSectionExamples sectionExample="quickLinks" />
          </div>

          <hr />
        </div>

        <div>
          {!showContents && (
            <div className={styles.stickyNavToggle}>
              <a
                href="#"
                className="govuk-link--no-underline govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowContents(true);
                }}
              >
                Show contents
              </a>
            </div>
          )}

          <div
            className={classNames({ [styles.releaseContainer]: showContents })}
          >
            {showContents && (
              <div className={styles.releaseNav}>
                <div className={styles.stickyLinksContainer}>
                  <h2
                    id="contentsNoSideNav"
                    className="govuk-body govuk-!-margin-bottom-1"
                  >
                    Contents
                  </h2>
                  <div className="govuk-body-s govuk-!-margin-bottom-6">
                    <a
                      href="#"
                      className="govuk-link--no-underline"
                      onClick={e => {
                        e.preventDefault();
                        setShowContents(!showContents);
                      }}
                    >
                      {showContents ? 'Hide contents' : 'Show'}
                    </a>
                  </div>
                  <div
                    className={classNames(styles.stickyLinks)}
                    style={{ border: 'none' }}
                  >
                    <div
                      className="dfe-flex dfe-justify-content--space-between"
                      style={{
                        flexDirection: 'column',
                      }}
                    >
                      <ul
                        className={classNames(
                          'govuk-list',
                          'govuk-list--spaced',
                          styles.prototypeInPageNavSpacing,
                        )}
                      >
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'Headlines',
                              },
                            )}
                            href="#Headlines"
                          >
                            Headline facts and figures
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'ExploreData',
                              },
                            )}
                            href="#exploreData"
                          >
                            Explore data used in this release
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'Summary',
                              },
                            )}
                            href="#Summary"
                          >
                            Methodologies and release details
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'About',
                              },
                            )}
                            href="#about"
                          >
                            About these statistics
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'Supplement',
                              },
                            )}
                            href="#supplement"
                          >
                            How to find data and supplementary tables
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'FullYearApprenticeship',
                              },
                            )}
                            href="#fullYearApprenticeships"
                          >
                            Full year apprenticeships data
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'FullYearTraineeship',
                              },
                            )}
                            href="#fullYearTraineeships"
                          >
                            Full year traineeships data
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'LatestApprenticeship',
                              },
                            )}
                            href="#latestApprenticeships"
                          >
                            Latest Apprenticeships in year data
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'LatestTraineeship',
                              },
                            )}
                            href="#latestTraineeships"
                          >
                            Latest Traineeships in year data
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'InteractiveTool',
                              },
                            )}
                            href="#interactiveTool"
                          >
                            Interactive data visualisation tool
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'PubSectorApprenticeship',
                              },
                            )}
                            href="#pubSectorApprenticeship"
                          >
                            Public sector apprenticeships 2022-23
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'AdditionalData',
                              },
                            )}
                            href="#additionalData"
                          >
                            Additional analysis and transparency data
                          </a>
                        </li>
                        <li>
                          <a
                            className={classNames(
                              'govuk-link--no-visited-state',
                              styles.prototypeLinkNoUnderline,
                              'govuk-body-s',
                              {
                                'govuk-!-font-weight-bold':
                                  visibleSection === 'Help',
                              },
                            )}
                            href="#help"
                          >
                            Help and support
                          </a>
                        </li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>
            )}

            <div className={styles.releaseMainContent}>
              <section id="Headlines" ref={headlinesRef}>
                <PrototypeSectionExamples sectionExample="headlines" />
              </section>
              <section id="ExploreData" ref={exploreRef}>
                <PrototypeSectionExamples sectionExample="explore" />
              </section>{' '}
              {isMobileMedia ? (
                <Accordion id="releaseContents">
                  <AccordionSection heading="Release details" backToTop>
                    <PrototypeSectionExamples sectionExample="summaryFull" />
                  </AccordionSection>
                  <AccordionSection
                    heading="How to find data and supplementary tables in this release"
                    backToTop
                  >
                    <PrototypeSectionExamples sectionExample="supplement" />
                  </AccordionSection>
                  <AccordionSection heading="Release details" backToTop>
                    <PrototypeSectionExamples sectionExample="summaryFull" />
                  </AccordionSection>
                  <AccordionSection
                    heading="Full year apprenticeship landscape"
                    backToTop
                  >
                    <PrototypeSectionExamples sectionExample="fullYearApprenticeship" />
                  </AccordionSection>
                  <AccordionSection heading="Full year traineeships" backToTop>
                    <PrototypeSectionExamples sectionExample="fullYearTraineeship" />
                  </AccordionSection>
                  <AccordionSection
                    heading="Latest apprenticeships in year data"
                    backToTop
                  >
                    <PrototypeSectionExamples sectionExample="latestApprenticeship" />
                  </AccordionSection>
                  <AccordionSection
                    heading="Latest traineeships in year data"
                    backToTop
                  >
                    <PrototypeSectionExamples sectionExample="latestTraineeship" />
                  </AccordionSection>
                  <AccordionSection
                    heading="Interactive data visualisation tool"
                    backToTop
                  >
                    <PrototypeSectionExamples sectionExample="interactiveTool" />
                  </AccordionSection>
                </Accordion>
              ) : (
                <>
                  <section id="Summary" ref={summaryRef}>
                    <PrototypeSectionExamples sectionExample="summaryFull" />
                  </section>
                  <div className={styles.stickyBackToTop}>
                    <a href="#">Back to top</a>
                  </div>
                  <section id="About" ref={aboutRef}>
                    <PrototypeSectionExamples sectionExample="about" />
                  </section>
                  <section id="Supplement" ref={supplementRef}>
                    <PrototypeSectionExamples sectionExample="supplement" />
                  </section>
                  <section
                    id="FullYearApprenticeship"
                    ref={fullYearApprenticeshipRef}
                  >
                    <PrototypeSectionExamples sectionExample="fullYearApprenticeship" />
                  </section>
                  <section
                    id="FullYearTraineeship"
                    ref={fullYearTraineeshiphRef}
                  >
                    <PrototypeSectionExamples sectionExample="fullYearTraineeship" />
                  </section>
                  <section
                    id="LatestApprenticeship"
                    ref={latestApprenticeshipRef}
                  >
                    <PrototypeSectionExamples sectionExample="latestApprenticeship" />
                  </section>
                  <section id="LatestTraineeship" ref={latestTraineeshipRef}>
                    <PrototypeSectionExamples sectionExample="latestTraineeship" />
                  </section>
                  <section id="interactiveTool" ref={interactiveToolRef}>
                    <PrototypeSectionExamples sectionExample="interactiveTool" />
                  </section>
                  <section
                    id="pubSectorApprenrticeship"
                    ref={pubSectorApprenticeshipRef}
                  >
                    <PrototypeSectionExamples sectionExample="pubSectorApprenticeship" />
                  </section>
                  <section id="additionalData" ref={addtionalDataRef}>
                    <PrototypeSectionExamples sectionExample="additionalData" />
                  </section>
                </>
              )}
              <section id="Help" ref={helpRef}>
                <PrototypeSectionExamples sectionExample="help" />
              </section>
            </div>
          </div>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
