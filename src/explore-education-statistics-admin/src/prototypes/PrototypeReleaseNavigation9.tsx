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
  const [showContents, setShowContents] = useState(true);
  const [showMainRelease, setShowMainRelease] = useState(true);
  const [showMethodologies, setShowMethodologies] = useState(false);
  const [showData, setShowData] = useState(false);
  const [showGuidance, setShowGuidance] = useState(false);
  const [showPreRelease, setShowPreRelease] = useState(false);
  const [showOtherReleases, setShowOtherReleases] = useState(false);
  const [showAllUpdates, setShowAllUpdates] = useState(false);
  const [showHelp, setShowHelp] = useState(false);
  const [showNationalStats, setShowNationalStats] = useState(false);

  const [visibleSection, setVisibleSection] = useState<string>();

  function viewUpdates() {
    setShowMainRelease(false);
    setShowMethodologies(false);
    setShowData(false);
    setShowGuidance(false);
    setShowPreRelease(false);
    setShowOtherReleases(false);
    setShowAllUpdates(true);
    setShowHelp(false);
    setShowNationalStats(false);
  }

  function viewGuidance() {
    setShowMainRelease(false);
    setShowMethodologies(false);
    setShowData(false);
    setShowGuidance(true);
    setShowPreRelease(false);
    setShowOtherReleases(false);
    setShowAllUpdates(false);
    setShowHelp(false);
    setShowNationalStats(false);
  }

  function viewNationalStats() {
    setShowMainRelease(false);
    setShowMethodologies(false);
    setShowData(false);
    setShowGuidance(false);
    setShowPreRelease(false);
    setShowOtherReleases(false);
    setShowAllUpdates(false);
    setShowHelp(false);
    setShowNationalStats(true);
  }

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

  const params = new URLSearchParams(window.location.search);
  const urlSection = params.get('section');

  let section = '';

  if (urlSection === 'main') {
    window.location.replace('/prototypes/release-nav9');
  }

  if (showMainRelease) {
    section = 'Latest data';
  }
  if (showData) {
    section = 'Explore data and create tables';
  }
  if (showAllUpdates) {
    section = 'Release archive and updates';
  }
  if (showMethodologies) {
    section = 'Methodologies';
  }
  if (showNationalStats) {
    section = 'What are National Statistics?';
  }
  if (showGuidance) {
    section = 'Data guidance';
  }
  if (showHelp) {
    section = 'Help, support and contact';
  }

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
            link: '/prototypes/release-nav9?section=main',
          },
          {
            name: section,
            link: '#',
          },
        ]}
        wide={false}
      >
        <div ref={headerRef}>
          <PrototypeSectionExamples
            sectionExample="header9"
            /* eslint-disable-next-line */
            change={viewUpdates}
            /* eslint-disable-next-line */
            change3={viewNationalStats}
          />
          <div className="govuk-!-margin-top-3">
            <nav className="govuk-!-margin-bottom-0" aria-label="Useful links">
              <h2 className="govuk-body govuk-visually-hidden">Useful links</h2>

              <ol
                className={classNames(styles.prototypeQuickLinks, 'govuk-list')}
                style={{ maxWidth: '100% !important' }}
              >
                <li>
                  <div
                    className={classNames(styles.prototypeActionLink, {
                      [styles.prototypeActionLinkOn]: showMainRelease,
                    })}
                  >
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#"
                        className={classNames('govuk-link--no-visited-state', {
                          'govuk-link--no-underline': showMainRelease,
                          'govuk-!-font-weight-bold': showMainRelease,
                        })}
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(true);
                          setShowMethodologies(false);
                          setShowGuidance(false);
                          setShowData(false);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(false);
                          setShowHelp(false);
                          setShowNationalStats(false);
                        }}
                      >
                        Main content page
                      </a>
                    </span>
                  </div>
                </li>
                <li>
                  <div
                    className={classNames(styles.prototypeActionLink, {
                      [styles.prototypeActionLinkOn]: showData,
                    })}
                  >
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#"
                        className={classNames('govuk-link--no-visited-state', {
                          'govuk-link--no-underline': showData,
                          'govuk-!-font-weight-bold': showData,
                        })}
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(false);
                          setShowGuidance(false);
                          setShowData(true);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(false);
                          setShowHelp(false);
                          setShowNationalStats(false);
                        }}
                      >
                        Explore data and create tables
                      </a>
                    </span>
                  </div>
                </li>
                <li>
                  <div
                    className={classNames(styles.prototypeActionLink, {
                      [styles.prototypeActionLinkOn]: showAllUpdates,
                    })}
                  >
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#releaseDetails"
                        className={classNames('govuk-link--no-visited-state', {
                          'govuk-link--no-underline': showAllUpdates,
                          'govuk-!-font-weight-bold': showAllUpdates,
                        })}
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(false);
                          setShowData(false);
                          setShowGuidance(false);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(true);
                          setShowHelp(false);
                          setShowNationalStats(false);
                        }}
                      >
                        Release archive and updates
                      </a>
                    </span>
                  </div>
                </li>

                <li>
                  <div
                    className={classNames(styles.prototypeActionLink, {
                      [styles.prototypeActionLinkOn]: showNationalStats,
                    })}
                  >
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#National statistics"
                        className={classNames('govuk-link--no-visited-state', {
                          'govuk-link--no-underline': showNationalStats,
                          'govuk-!-font-weight-bold': showNationalStats,
                        })}
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(false);
                          setShowData(false);
                          setShowGuidance(false);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(false);
                          setShowHelp(false);
                          setShowNationalStats(true);
                        }}
                      >
                        What are National Statistics?
                      </a>
                    </span>
                  </div>
                </li>
                <li>
                  <div
                    className={classNames(styles.prototypeActionLink, {
                      [styles.prototypeActionLinkOn]: showMethodologies,
                    })}
                  >
                    <span className={styles.prototypeActionLinkWrapperDownload}>
                      <a
                        href="#releaseDetails"
                        className={classNames('govuk-link--no-visited-state', {
                          'govuk-link--no-underline': showMethodologies,
                          'govuk-!-font-weight-bold': showMethodologies,
                        })}
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(true);
                          setShowData(false);
                          setShowGuidance(false);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(false);
                          setShowHelp(false);
                          setShowNationalStats(false);
                        }}
                      >
                        Methodologies
                      </a>
                    </span>
                  </div>
                </li>

                {/* 
                <li>
                  <div className={styles.prototypeActionLink}>
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#"
                        className="govuk-link--no-visited-state"
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(false);
                          setShowData(false);
                          setShowGuidance(true);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(false);
                          setShowHelp(false);
                        }}
                      >
                        Data guidance
                      </a>
                    </span>
                  </div>
                </li>
    
                <li>
                  <div className={styles.prototypeActionLink}>
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#"
                        className="govuk-link--no-visited-state"
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(false);
                          setShowGuidance(false);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(true);
                          setShowHelp(false);
                        }}
                      >
                        View all updates
                      </a>
                    </span>
                  </div>
                </li>
              
                    
                <li>
                  <div className={styles.prototypeActionLink}>
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#"
                        className="govuk-link--no-visited-state"
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(false);
                          setShowData(false);
                          setShowGuidance(false);
                          setShowPreRelease(true);
                          setShowOtherReleases(false);
                          setShowAllUpdates(false);
                          setShowHelp(false);
                        }}
                      >
                        Pre release access list
                      </a>
                    </span>
                  </div>
                </li>
                
                */}

                <li>
                  <div
                    className={classNames(styles.prototypeActionLink, {
                      [styles.prototypeActionLinkOn]: showHelp,
                    })}
                  >
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a
                        href="#help"
                        className={classNames('govuk-link--no-visited-state', {
                          'govuk-link--no-underline': showHelp,
                          'govuk-!-font-weight-bold': showHelp,
                        })}
                        onClick={e => {
                          e.preventDefault();
                          setShowMainRelease(false);
                          setShowMethodologies(false);
                          setShowGuidance(false);
                          setShowData(false);
                          setShowPreRelease(false);
                          setShowOtherReleases(false);
                          setShowAllUpdates(false);
                          setShowHelp(true);
                          setShowNationalStats(false);
                        }}
                      >
                        Help, support and contact
                      </a>
                    </span>
                  </div>
                </li>
                <li>
                  <div
                    className={classNames(
                      styles.prototypeActionLink,
                      styles.prototypeNotifyLink,
                    )}
                  >
                    <span className={styles.prototypeActionLinkWrapper}>
                      <a href="#" className="govuk-link--no-visited-state">
                        Sign up for email alerts
                      </a>
                    </span>
                  </div>
                </li>
                {!isMobileMedia && (
                  <li>
                    <div
                      className={classNames(
                        styles.prototypeActionLink,
                        styles.prototypeDownloadLink,
                      )}
                    >
                      <span className={styles.prototypeActionLinkWrapper}>
                        <a href="#" className="govuk-link--no-visited-state">
                          Download all data (ZIP)
                        </a>
                      </span>
                    </div>
                  </li>
                )}
              </ol>
            </nav>
          </div>

          <hr />
        </div>

        <div>
          {isMobileMedia && showMainRelease && (
            <div className={styles.stickyNavToggle}>
              <h2 className="govuk-body govuk-!-margin-bottom-0">
                <a
                  href="#releaseContents"
                  className="govuk-link--no-underline"
                  onClick={() => {
                    setShowContents(true);
                  }}
                >
                  On this page
                </a>
              </h2>
            </div>
          )}

          <div
            className={classNames({
              [styles.releaseContainer]: !isMobileMedia,
            })}
          >
            <div className={styles.releaseNav}>
              <div className={styles.stickyLinksContainer}>
                {!showMainRelease && (
                  <div className={classNames(styles.stickyLinks)}>
                    <div
                      className={classNames(
                        styles.prototypeActionLink,
                        styles.prototypeBackLink,
                      )}
                    >
                      <span className={styles.prototypeActionLinkWrapper}>
                        <a
                          href="#"
                          className="govuk-link--no-visited-state"
                          onClick={() => {
                            setShowMainRelease(true);
                            setShowMethodologies(false);
                            setShowGuidance(false);
                            setShowData(false);
                            setShowPreRelease(false);
                            setShowOtherReleases(false);
                            setShowAllUpdates(false);
                            setShowHelp(false);
                            setShowNationalStats(false);
                          }}
                        >
                          Back to main content
                        </a>
                      </span>
                    </div>
                    {showAllUpdates && (
                      <>
                        <h2 className="govuk-body govuk-!-margin-top-6">
                          On this page
                        </h2>
                        <nav
                          style={{ border: 'none' }}
                          aria-label="Page contents"
                        >
                          <ul
                            role="navigation"
                            className={classNames(
                              'govuk-list',
                              'govuk-list--spaced',
                              styles.prototypeInPageNavSpacing,
                              {
                                [styles.prototypeMobileContents]: isMobileMedia,
                              },
                            )}
                            id="releaseContents"
                          >
                            <li>
                              <a
                                className={classNames(
                                  'govuk-link--no-visited-state',
                                  styles.prototypeLinkNoUnderline,
                                  'govuk-body-s',
                                )}
                                href="#releasesInSeries"
                              >
                                Releases in this series
                              </a>
                            </li>
                            <li>
                              <a
                                className={classNames(
                                  'govuk-link--no-visited-state',
                                  styles.prototypeLinkNoUnderline,
                                  'govuk-body-s',
                                )}
                                href="#viewUpdates"
                              >
                                Updates to this release
                              </a>
                            </li>
                          </ul>
                        </nav>
                      </>
                    )}
                    {showHelp && (
                      <>
                        <h2 className="govuk-body govuk-!-margin-top-6">
                          On this page
                        </h2>
                        <nav
                          style={{ border: 'none' }}
                          aria-label="Page contents"
                        >
                          <ul
                            role="navigation"
                            className={classNames(
                              'govuk-list',
                              'govuk-list--spaced',
                              styles.prototypeInPageNavSpacing,
                              {
                                [styles.prototypeMobileContents]: isMobileMedia,
                              },
                            )}
                            id="releaseContents"
                          >
                            <li>
                              <a
                                className={classNames(
                                  'govuk-link--no-visited-state',
                                  styles.prototypeLinkNoUnderline,
                                  'govuk-body-s',
                                )}
                                href="#methodology"
                              >
                                Methodology
                              </a>
                            </li>
                            <li>
                              <a
                                className={classNames(
                                  'govuk-link--no-visited-state',
                                  styles.prototypeLinkNoUnderline,
                                  'govuk-body-s',
                                )}
                                href="#contactUs"
                              >
                                Contact us
                              </a>
                            </li>
                            <li>
                              <a
                                className={classNames(
                                  'govuk-link--no-visited-state',
                                  styles.prototypeLinkNoUnderline,
                                  'govuk-body-s',
                                )}
                                href="#preRelease"
                              >
                                Pre-release access list
                              </a>
                            </li>
                          </ul>
                        </nav>
                      </>
                    )}
                  </div>
                )}

                {showMainRelease && (
                  <>
                    {!isMobileMedia && (
                      <h2 id="contents" className="govuk-body">
                        On this page
                      </h2>
                    )}

                    {/*
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
                      */}
                    <nav
                      className={classNames(styles.stickyLinks)}
                      style={{ border: 'none' }}
                      aria-label="Page contents"
                    >
                      <div
                        className="dfe-flex dfe-justify-content--space-between"
                        style={{
                          flexDirection: 'column',
                        }}
                      >
                        {(!isMobileMedia || showContents) && (
                          <>
                            <ul
                              role="navigation"
                              className={classNames(
                                'govuk-list',
                                'govuk-list--spaced',
                                styles.prototypeInPageNavSpacing,
                                {
                                  [styles.prototypeMobileContents]:
                                    isMobileMedia,
                                },
                              )}
                              id="releaseContents"
                            >
                              <li>
                                <a
                                  className={classNames(
                                    'govuk-link--no-visited-state',
                                    styles.prototypeLinkNoUnderline,
                                    'govuk-body-s',
                                    {
                                      [styles.prototypeInPageNavOn]:
                                        visibleSection === 'Headlines',
                                    },
                                  )}
                                  href="#Headlines"
                                >
                                  Headline facts and figures
                                </a>
                              </li>
                              {/* 
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
                            */}

                              <li>
                                <a
                                  className={classNames(
                                    'govuk-link--no-visited-state',
                                    styles.prototypeLinkNoUnderline,
                                    'govuk-body-s',
                                    {
                                      [styles.prototypeInPageNavOn]:
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
                                      [styles.prototypeInPageNavOn]:
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
                                      [styles.prototypeInPageNavOn]:
                                        visibleSection ===
                                        'FullYearApprenticeship',
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
                                      [styles.prototypeInPageNavOn]:
                                        visibleSection ===
                                        'FullYearTraineeship',
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
                                      [styles.prototypeInPageNavOn]:
                                        visibleSection ===
                                        'LatestApprenticeship',
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
                                      [styles.prototypeInPageNavOn]:
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
                                      [styles.prototypeInPageNavOn]:
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
                                      [styles.prototypeInPageNavOn]:
                                        visibleSection ===
                                        'PubSectorApprenticeship',
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
                                      [styles.prototypeInPageNavOn]:
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
                                      [styles.prototypeInPageNavOn]:
                                        visibleSection === 'Help',
                                    },
                                  )}
                                  href="#help"
                                >
                                  Pre-release access list
                                </a>
                              </li>
                            </ul>
                            <div
                              className={classNames(
                                styles.prototypeActionLink,
                                styles.prototypeBackToTopLink,
                                'govuk-!-margin-top-6',
                              )}
                            >
                              <span
                                className={styles.prototypeActionLinkWrapper}
                              >
                                <a
                                  href="#"
                                  className="govuk-link--no-visited-state"
                                  onClick={() => {
                                    setShowOtherReleases(false);
                                    setShowMainRelease(true);
                                  }}
                                >
                                  Back to top
                                </a>
                              </span>
                            </div>
                          </>
                        )}
                      </div>
                    </nav>
                  </>
                )}
              </div>
            </div>

            <div className={styles.releaseMainContent}>
              {showMainRelease && (
                <>
                  <section
                    id="Headlines"
                    ref={headlinesRef}
                    aria-label="Headline facts and figures"
                  >
                    <PrototypeSectionExamples sectionExample="headlines2" />
                  </section>

                  <hr className={styles.prototypeSectionBreak} />

                  {isMobileMedia ? (
                    <Accordion id="releaseContents">
                      <AccordionSection
                        heading="About these statistics"
                        backToTop
                      >
                        <PrototypeSectionExamples sectionExample="about" />
                      </AccordionSection>{' '}
                      <AccordionSection
                        heading="How to find data and supplementary tables in this release"
                        backToTop
                      >
                        <PrototypeSectionExamples sectionExample="supplement" />
                      </AccordionSection>
                      <AccordionSection
                        heading="Full year apprenticeship landscape"
                        backToTop
                      >
                        <PrototypeSectionExamples sectionExample="fullYearApprenticeship" />
                      </AccordionSection>
                      <AccordionSection
                        heading="Full year traineeships"
                        backToTop
                      >
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
                      <section
                        id="About"
                        ref={aboutRef}
                        aria-label="About these statistics"
                      >
                        <PrototypeSectionExamples sectionExample="about" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="Supplement"
                        ref={supplementRef}
                        aria-label="How to find data and supplementary tables"
                      >
                        <PrototypeSectionExamples sectionExample="supplement" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="FullYearApprenticeship"
                        ref={fullYearApprenticeshipRef}
                        aria-label="Full year apprenticeships data"
                      >
                        <PrototypeSectionExamples sectionExample="fullYearApprenticeship" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="FullYearTraineeship"
                        ref={fullYearTraineeshiphRef}
                        aria-label="Full year traineeships data"
                      >
                        <PrototypeSectionExamples sectionExample="fullYearTraineeship" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="LatestApprenticeship"
                        ref={latestApprenticeshipRef}
                        aria-label="Latest Apprenticeships in year data"
                      >
                        <PrototypeSectionExamples sectionExample="latestApprenticeship" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="LatestTraineeship"
                        ref={latestTraineeshipRef}
                        aria-label="Latest Traineeships in year data"
                      >
                        <PrototypeSectionExamples sectionExample="latestTraineeship" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="interactiveTool"
                        ref={interactiveToolRef}
                        aria-label="Interactive data visualisation tool"
                      >
                        <PrototypeSectionExamples sectionExample="interactiveTool" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="pubSectorApprenrticeship"
                        ref={pubSectorApprenticeshipRef}
                        aria-label="Public sector apprenticeships 2022-23"
                      >
                        <PrototypeSectionExamples sectionExample="pubSectorApprenticeship" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section
                        id="additionalData"
                        ref={addtionalDataRef}
                        aria-label="Additional analysis and transparency data"
                      >
                        <PrototypeSectionExamples sectionExample="additionalData" />
                      </section>
                      <hr className={styles.prototypeSectionBreak} />
                      <section id="Summary" ref={summaryRef}>
                        <PrototypeSectionExamples sectionExample="summaryFull" />
                      </section>
                    </>
                  )}
                  <section
                    id="help"
                    ref={helpRef}
                    aria-label="Help and support"
                  >
                    <PrototypeSectionExamples sectionExample="preRelease" />
                  </section>
                </>
              )}
              {showOtherReleases && (
                <>
                  <PrototypeSectionExamples sectionExample="otherReleases" />
                  <PrototypeSectionExamples sectionExample="allUpdates" />
                </>
              )}

              {showAllUpdates && (
                <>
                  <PrototypeSectionExamples sectionExample="otherReleases" />
                  <PrototypeSectionExamples sectionExample="allUpdates" />
                </>
              )}

              {showMethodologies && (
                <PrototypeSectionExamples sectionExample="methodologies" />
              )}
              {showData && (
                <PrototypeSectionExamples
                  sectionExample="explore2" /* eslint-disable-next-line */
                  change2={viewGuidance}
                />
              )}
              {showGuidance && (
                <PrototypeSectionExamples sectionExample="guidance" />
              )}
              {showPreRelease && (
                <PrototypeSectionExamples sectionExample="preRelease" />
              )}
              {showNationalStats && (
                <PrototypeSectionExamples sectionExample="nationalStats" />
              )}
              {showHelp && (
                <>
                  <PrototypeSectionExamples sectionExample="help" />
                  <PrototypeSectionExamples sectionExample="preRelease" />
                </>
              )}
            </div>
          </div>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
