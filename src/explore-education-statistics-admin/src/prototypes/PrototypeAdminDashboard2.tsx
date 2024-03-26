import classNames from 'classnames';
import PageTitle from '@admin/components/PageTitle';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import Tag from '@common/components/Tag';
import TabsSection from '@common/components/TabsSection';
import RelatedInformation from '@common/components/RelatedInformation';
import Button from '@common/components/Button';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import ModalContent from '@admin/prototypes/components/PrototypeModalContent';

const PrototypeManageUsers = () => {
  const [showCreatePub, setShowCreatePub] = useState(true);
  const [showBau, setShowBau] = useState(false);
  const [showCreatePublicationModal, toggleCreatePublicationModal] =
    useToggle(false);
  const [showHelpStatusModal, toggleHelpStatusModal] = useToggle(false);
  const [showHelpIssuesModal, toggleHelpIssuesModal] = useToggle(false);
  const [showScheduledStatusModal, toggleScheduledStatusModal] =
    useToggle(false);
  const [showScheduledStagesModal, toggleScheduledStagesModal] =
    useToggle(false);
  const [theme, setTheme] = useState('All themes');
  const [themeValue, setThemeValue] = useState('theme-1');

  // const [currentPage, setCurrentPage] = useState<number>(0);
  // const [totalResults, setTotalResults] = useState<number>();
  // const [selectedSortOrder, setSelectedSortOrder] = useState('newest');

  return (
    <PrototypePage
      wide
      breadcrumbs={[{ name: 'Dashboard', link: '/dashboard' }]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle
            title="Dashboard"
            caption={showBau ? 'Welcome Bau1' : 'Welecome Standard admin user'}
          />
          <p className="govuk-body-s">
            Logged in as{' '}
            <strong>{showBau ? 'Bau1' : 'Standard admin user'}</strong>. Not
            you?{' '}
            <a className="govuk-link govuk-link" href="/authentication/logout">
              Sign out
            </a>
          </p>
          <p className="govuk-hint govuk-!-margin-bottom-6">
            This is your administration dashboard, here you can manage
            publications, releases and methodologies.
          </p>
          {showBau && (
            <ul className="govuk-!-margin-bottom-6">
              <li>
                <a href="#">manage themes and topics</a>
              </li>
            </ul>
          )}
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/contact-us" target="_blank">
                  Contact us
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <Tabs id="manage-release-users">
        <TabsSection
          title={showBau ? 'All publications' : 'Your publications (8)'}
        >
          {!showBau && (
            <>
              <h2>View and manage your publications </h2>
              <p>Select a publication to:</p>
              <ul className="govuk-list--bullet">
                <li>create new releases and methodologies</li>
                <li>edit existing releases and methodologies</li>
                <li>view and sign-off releases and methodologies</li>
              </ul>{' '}
              <hr />
            </>
          )}

          {showBau && (
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-quarter">
                <form action="#">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="searchTerm"
                      className="govuk-label govuk-label--m"
                    >
                      Search
                    </label>
                    <div className="dfe-flex">
                      <input
                        type="search"
                        name="search"
                        className="govuk-input"
                      />
                      <button
                        style={{
                          background: '#1d70b8',
                          color: '#FFFFFF',
                          cursor: 'pointer',
                          height: '40px',
                          width: '40px',
                          padding: '0',
                        }}
                        type="submit"
                      >
                        <svg
                          aria-hidden="true"
                          className="SearchForm_icon__TJ9Bw"
                          focusable="false"
                          xmlns="http://www.w3.org/2000/svg"
                          viewBox="0 0 36 36"
                          width="36"
                          height="36"
                        >
                          <path
                            d="M25.7 24.8L21.9 21c.7-1 1.1-2.2 1.1-3.5 0-3.6-2.9-6.5-6.5-6.5S10 13.9 10 17.5s2.9 6.5 6.5 6.5c1.6 0 3-.6 4.1-1.5l3.7 3.7 1.4-1.4zM12 17.5c0-2.5 2-4.5 4.5-4.5s4.5 2 4.5 4.5-2 4.5-4.5 4.5-4.5-2-4.5-4.5z"
                            fill="currentColor"
                          />
                        </svg>
                        <span className="govuk-visually-hidden">Search</span>
                      </button>
                    </div>
                  </div>
                </form>

                <fieldset className="govuk-fieldset govuk-!-margin-top-6">
                  <legend className="govuk-heading-s govuk-!-margin-bottom-0 ">
                    Filter by theme
                  </legend>
                  <div className="govuk-radios govuk-radios--small">
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-1"
                        onClick={() => {
                          setTheme('All themes');
                          setThemeValue('theme-1');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-1"
                      >
                        All themes
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-2"
                        checked={theme === 'theme-2'}
                        onClick={() => {
                          setTheme("Children's social care");
                          setThemeValue('theme-2');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-2"
                      >
                        {' '}
                        Children's social care
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-3"
                        onClick={() => {
                          setTheme('COVID-19');
                          setThemeValue('theme-3');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-3"
                      >
                        COVID-19
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-4"
                        onClick={() => {
                          setTheme('Destination of pupils and students');
                          setThemeValue('theme-4');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-4"
                      >
                        Destination of pupils and students
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-5"
                        onClick={() => {
                          setTheme('Early years');
                          setThemeValue('theme-5');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-5"
                      >
                        Early years
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-6"
                        onClick={() => {
                          setTheme('Finance and funding');
                          setThemeValue('theme-6');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-6"
                      >
                        Finance and funding
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-7"
                        onClick={() => {
                          setTheme('Higher education');
                          setThemeValue('theme-7');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-7"
                      >
                        Higher education
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-8"
                        onClick={() => {
                          setTheme('Pupils and schools');
                          setThemeValue('theme-8');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-8"
                      >
                        Pupils and schools
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-9"
                        onClick={() => {
                          setTheme(
                            'Schools and college outcomes and performance',
                          );
                          setThemeValue('theme-9');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-9"
                      >
                        Schools and college outcomes and performance
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-10"
                        onClick={() => {
                          setTheme('Teachers and school workforce');
                          setThemeValue('theme-10');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-10"
                      >
                        Teachers and school workforce
                      </label>
                    </div>
                    <div className="govuk-radios__item">
                      <input
                        type="radio"
                        className="govuk-radios__input"
                        name="theme"
                        id="theme-11"
                        onClick={() => {
                          setTheme('UK education and training statistics');
                          setThemeValue('theme-11');
                        }}
                      />
                      <label
                        className={classNames(
                          'govuk-label',
                          'govuk-radios__label',
                        )}
                        htmlFor="theme-11"
                      >
                        UK education and training statistics
                      </label>
                    </div>
                  </div>
                </fieldset>
              </div>
              <div className="govuk-grid-column-three-quarters">
                <h2>{theme}</h2>
                <div className="dfe-flex dfe-justify-content--space-between">
                  <div className="govuk-form-group govuk-!-margin-bottom-3">
                    <fieldset className="govuk-fieldset">
                      <legend className="govuk-fieldset__legend govuk-fieldset__legend--s govuk-!-margin-bottom-0">
                        Sort results
                      </legend>
                      <div className="govuk-radios govuk-radios--small  govuk-radios--inline">
                        <div className="govuk-radios__item">
                          <input
                            type="radio"
                            className="govuk-radios__input"
                            name="sort"
                            id="sort-3"
                            checked
                          />
                          <label
                            className={classNames(
                              'govuk-label',
                              'govuk-radios__label',
                            )}
                            htmlFor="sort-3"
                          >
                            A to Z
                          </label>
                        </div>
                        <div className="govuk-radios__item">
                          <input
                            type="radio"
                            className="govuk-radios__input"
                            name="sort"
                            id="sort-1"
                          />
                          <label
                            className={classNames(
                              'govuk-label',
                              'govuk-radios__label',
                            )}
                            htmlFor="sort-1"
                          >
                            Newest
                          </label>
                        </div>
                        <div className="govuk-radios__item">
                          <input
                            type="radio"
                            className="govuk-radios__input"
                            name="sort"
                            id="sort-2"
                          />
                          <label
                            className={classNames(
                              'govuk-label',
                              'govuk-radios__label',
                            )}
                            htmlFor="sort-2"
                          >
                            Oldest
                          </label>
                        </div>
                      </div>
                    </fieldset>
                  </div>
                  {themeValue === 'theme-1' && (
                    <div>
                      <Button
                        onClick={() => {
                          toggleCreatePublicationModal(true);
                        }}
                      >
                        Create a new publication
                      </Button>
                    </div>
                  )}
                </div>

                <table>
                  <thead>
                    <tr>
                      <th>Publication</th>
                      <th
                        className="govuk-table__cell--numeric govuk-table__header"
                        style={{ width: '180px', fontWeight: 'bold' }}
                      >
                        Last release date
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {themeValue === 'theme-1' && (
                      <>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              16-18 destination measures
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            20 Oct 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              A level and other 16 to 18 results
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            30 Mar 2023
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Academy transfers and funding
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            21 Jul 2023
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Admission appeals in England
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            18 Aug 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Apprenticeships and traineeships
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            13 Apr 2023
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Attendance in education and early years settings
                              during the coronavirus (COVID-19) pandemic
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            28 Apr 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Career pathways: post-16 qualifications held by
                              employees
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            25 May 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Characteristics of children in need
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            26 Jul 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Childcare and early years provider survey
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            25 May 2022
                          </td>
                        </tr>
                      </>
                    )}
                    {themeValue === 'theme-2' && (
                      <>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Characteristics of children in need
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            27 Oct 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Children accommodated in secure children's homes
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            26 May 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Children looked after in England including
                              adoptions
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            17 Nov 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Children's social work workforce
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            23 Feb 2023
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Looked after children aged 16 to 17 in independent
                              or semi-independent placements
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            23 May 2022
                          </td>
                        </tr>
                      </>
                    )}
                    {themeValue === 'theme-3' && (
                      <>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Attendance in education and early years settings
                              during the coronavirus (COVID-19) pandemic
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            26 Jul 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              CO2 monitors: cumulative delivery statistics
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            16 Dec 2021
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Coronavirus (COVID-19) Reporting in Higher
                              Education Providers
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            19 Feb 2021
                          </td>
                        </tr>
                      </>
                    )}
                    {themeValue === 'theme-4' && (
                      <>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              16-18 destination measures
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            20 Oct 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Further education: outcome-based success measures
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            24 Oct 2022
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Link to="/prototypes/admin-publication">
                              Key stage 4 destination measures
                            </Link>
                          </td>
                          <td className="govuk-table__cell--numeric">
                            24 Nov 2022
                          </td>
                        </tr>
                      </>
                    )}
                  </tbody>
                </table>

                {themeValue === 'theme-1' && (
                  <nav className="govuk-pagination govuk-!-margin-top-9">
                    <ul className="govuk-pagination__list">
                      <li className="govuk-pagination__item govuk-pagination__item--current">
                        <a
                          href="#"
                          className="govuk-link govuk-pagination__link"
                        >
                          1
                        </a>
                      </li>
                      <li className="govuk-pagination__item">
                        <a
                          href="#"
                          className="govuk-link govuk-pagination__link"
                        >
                          2
                        </a>
                      </li>
                      <li className="govuk-pagination__item">
                        <a
                          href="#"
                          className="govuk-link govuk-pagination__link"
                        >
                          3
                        </a>
                      </li>
                      <li className="govuk-pagination__item">
                        <a
                          href="#"
                          className="govuk-link govuk-pagination__link"
                        >
                          4
                        </a>
                      </li>
                      <li className="govuk-pagination__item">
                        <a
                          href="#"
                          className="govuk-link govuk-pagination__link"
                        >
                          5
                        </a>
                      </li>
                    </ul>

                    <div className="govuk-pagination__next">
                      <a href="#">Next</a>
                      <svg
                        className="govuk-pagination__icon govuk-pagination__icon--next"
                        xmlns="http://www.w3.org/2000/svg"
                        height="13"
                        width="15"
                        aria-hidden="true"
                        focusable="false"
                        viewBox="0 0 15 13"
                      >
                        <path d="m8.107-0.0078125-1.4136 1.414 4.2926 4.293h-12.986v2h12.896l-4.1855 3.9766 1.377 1.4492 6.7441-6.4062-6.7246-6.7266z" />
                      </svg>
                    </div>
                  </nav>
                )}
                {themeValue !== 'theme-1' && (
                  <div className="dfe-align--right">
                    <Button>Create new publication for {theme}</Button>
                  </div>
                )}
              </div>
            </div>
          )}

          {!showBau && (
            <>
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-three-quarters">
                  <h3>Pupils and schools</h3>
                  <ul className="govuk-list govuk-list--spaced">
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Permanent and fixed-period exclusions in England
                      </Link>
                    </li>
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Pupil absence in schools in England
                      </Link>
                    </li>
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Pupil absence in schools in England: autumn and spring
                      </Link>
                    </li>
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Pupil absence in schools in England: autumn term
                      </Link>
                    </li>
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Secondary and primary school applications and offers
                      </Link>
                    </li>
                  </ul>
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <div className="dfe-align--right">
                    {showCreatePub && (
                      <a href="#" className="govuk-button">
                        Create new publication
                      </a>
                    )}
                  </div>
                </div>
              </div>
              <hr />
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-three-quarters">
                  <h3>School and college outcomes and performance</h3>
                  <ul className="govuk-list govuk-list--spaced">
                    <li>
                      <Link to="/prototypes/admin-publication">
                        A level and other 16 to 18 results
                      </Link>
                    </li>
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Key stage 1 and phonics screening check attainment
                      </Link>
                    </li>
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Key stage 2 attainment
                      </Link>
                    </li>
                  </ul>
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <div className="dfe-align--right">
                    {showCreatePub && (
                      <a href="#" className="govuk-button">
                        Create new publication
                      </a>
                    )}
                  </div>
                </div>
              </div>
            </>
          )}
        </TabsSection>
        <TabsSection title="Draft releases (4)">
          <h2>Draft releases</h2>
          <p className="govuk-hint govuk-!-width-three-quarters">
            Here you can view and edit any of your releases that are currently
            in 'Draft' or 'In review' and also 'Amendments' that are being made
            to a published release. You can also view a summary of any
            outstanding issues that may need to be resolved.
          </p>
          <div style={{ width: '100%', overflow: 'auto' }}>
            <table className="govuk-table">
              <caption className="govuk-visually-hidden">
                Table showing your draft releases
              </caption>
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th style={{ width: '35%' }}>Publication / Release period</th>
                  <th style={{ width: '8%' }}>
                    Status
                    <a
                      href="#"
                      className="govuk-!-margin-left-1"
                      onClick={() => {
                        toggleHelpStatusModal(true);
                      }}
                    >
                      <InfoIcon description="What is status?" />
                    </a>
                  </th>
                  <th style={{ width: '47%' }}>
                    Issues
                    <a
                      href="#"
                      className="govuk-!-margin-left-1"
                      onClick={() => {
                        toggleHelpIssuesModal(true);
                      }}
                    >
                      <InfoIcon description="What are issues?" />
                    </a>
                  </th>
                  <th className="govuk-table__cell--numeric">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England
                  </th>
                </tr>
                <tr>
                  <td>Academic Year 2020/21 (Not live)</td>
                  <td style={{ width: '12%' }}>
                    <Tag>Draft</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (9)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex dfe-justify-content--space-between">
                        <li>
                          <Tag colour="red">3 Errors</Tag>
                        </li>
                        <li>
                          <Tag colour="yellow">3 Warnings</Tag>
                        </li>
                        <li>
                          <Tag colour="grey">3 Unresolved comments</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
                <tr>
                  <td>Academic Year 2019/20 (Not live)</td>
                  <td>
                    <Tag>In review</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (1)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex dfe-justify-content--space-between">
                        <li>
                          <Tag colour="yellow">1 Warning</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England: autumn and spring
                  </th>
                </tr>
                <tr>
                  <td>Academic Year 2020/21 (Not live)</td>
                  <td>
                    <Tag>Draft</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (9)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex dfe-justify-content--space-between">
                        <li>
                          <Tag colour="red">3 Errors</Tag>
                        </li>
                        <li>
                          <Tag colour="yellow">3 Warnings</Tag>
                        </li>
                        <li>
                          <Tag colour="grey">3 Unresolved comments</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-9">
                    Pupil absence in schools in England: autumn term
                  </th>
                </tr>
                <tr>
                  <td>Academic Year 2020/21 (Not live)</td>
                  <td>
                    <Tag>Amendment</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (3)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex">
                        <li>
                          <Tag colour="yellow">3 Warnings</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
              </tbody>
            </table>
            <Modal
              open={showHelpStatusModal}
              title="Draft status guidance"
              className="govuk-!-width-one-half"
            >
              <ModalContent contentType="helpStatusModal" />
              <Button
                onClick={() => {
                  toggleHelpStatusModal(false);
                }}
              >
                Close
              </Button>
            </Modal>
            <Modal
              open={showHelpIssuesModal}
              title="Issues guidance"
              className="govuk-!-width-one-half"
            >
              <ModalContent contentType="helpIssuesModal" />
              <Button
                onClick={() => {
                  toggleHelpIssuesModal(false);
                }}
              >
                Close
              </Button>
            </Modal>
          </div>
        </TabsSection>
        <TabsSection title="Approved scheduled releases (3)">
          <h2>Approved scheduled releases</h2>
          <p className="govuk-hint govuk-!-width-three-quarters govuk-!-margin-bottom-6">
            Here you can view releases that have been approved and are now
            scheduled for publication. You can also check the progress of any
            releases that are currently being published to live.
          </p>
          <div style={{ width: '100%', overflow: 'auto' }}>
            <table className="govuk-table">
              <caption className="govuk-visually-hidden">
                View approved scheduled releases
              </caption>
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th style={{ width: '38%' }}>Publication / Release period</th>
                  <th>
                    Status
                    <a
                      href="#"
                      className="govuk-!-margin-left-1"
                      onClick={() => {
                        toggleScheduledStatusModal(true);
                      }}
                    >
                      <InfoIcon description="What are the publication stages?" />
                    </a>
                  </th>
                  <th style={{ width: '315px' }}>
                    Stages checklist
                    <a
                      href="#"
                      className="govuk-!-margin-left-1"
                      onClick={() => {
                        toggleScheduledStagesModal(true);
                      }}
                    >
                      <InfoIcon description="What is status?" />
                    </a>
                  </th>
                  <th>Scheduled publish date</th>
                  <th colSpan={2} className="govuk-table__cell--numeric">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <th colSpan={7} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England
                  </th>
                </tr>
                <tr>
                  <td>
                    Academic Year 2020/21 (Not live) <Tag>Amendment</Tag>
                  </td>
                  <td>
                    <Tag colour="orange">Validating</Tag>
                  </td>
                  <td>-</td>
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>

                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England: autumn and spring
                  </th>
                </tr>
                <tr>
                  <td>Academic Year 2020/21 (Not live)</td>
                  <td>
                    <Tag colour="blue">Scheduled</Tag>
                  </td>
                  <td>-</td>
                  <td>20 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>

                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England: autumn term
                  </th>
                </tr>
                <tr>
                  <td>Academic Year 2018/19 (Not live)</td>
                  <td>
                    <Tag colour="orange">STARTED</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View stages"
                      className="govuk-!-margin-bottom-0"
                    >
                      <h4>Release process started</h4>
                      <ul className="govuk-list">
                        <li>
                          <Tag colour="orange">Data Started</Tag>
                        </li>
                        <li>
                          <Tag colour="blue">Content not started</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Files complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="blue">Publishing not started</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>
                <tr>
                  <td>Academic Year 2019/20 (Not live)</td>
                  <td>
                    <Tag colour="green">COMPLETE</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View stages"
                      className="govuk-!-margin-bottom-0"
                    >
                      <h4>Publishing complete</h4>
                      <ul className="govuk-list">
                        <li>
                          <Tag colour="green">Data complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Content complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Files complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Publishing complete ✓</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>
                <tr>
                  <td>Academic Year 2021/22 (Not live)</td>
                  <td>
                    <Tag colour="red">FAILED</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View stages"
                      className="govuk-!-margin-bottom-0"
                    >
                      <h4>Publishing cancelled</h4>
                      <ul className="govuk-list">
                        <li>
                          <Tag colour="red">Data failed ✖</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Content complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Files complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="red">Publishing cancelled ✖</Tag>
                        </li>
                      </ul>
                      <h5 className="govuk-!-margin-0">Help and guidance</h5>
                      <p className="govuk-body-s">
                        For extra help and guidance to help rectify this issue
                        please email:
                        <br />
                        <a href="#">explore.statistics@education.gov.uk</a>
                      </p>
                    </Details>
                  </td>
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic Year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>
              </tbody>
            </table>
            <Modal
              open={showScheduledStatusModal}
              title="Status guidance"
              className="govuk-!-width-one-half"
            >
              <ModalContent contentType="scheduledStatusModal" />
              <Button
                onClick={() => {
                  toggleScheduledStatusModal(false);
                }}
              >
                Close
              </Button>
            </Modal>
            <Modal
              open={showScheduledStagesModal}
              title="Publication stages guidance"
              className="govuk-!-width-one-half"
            >
              <ModalContent contentType="scheduledStagesModal" />
              <Button
                onClick={() => {
                  toggleScheduledStagesModal(false);
                }}
              >
                Close
              </Button>
            </Modal>
          </div>
        </TabsSection>
      </Tabs>
      <div className="dfe-align--right govuk-!-margin-top-9">
        <ul className="govuk-list">
          <li>
            {showCreatePub ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowCreatePub(false);
                }}
              >
                Remove create role
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowCreatePub(true);
                }}
              >
                Add create role
              </a>
            )}
          </li>
          <li>
            {showBau ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowBau(false);
                }}
              >
                Remove BAU role
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowBau(true);
                }}
              >
                Add BAU role
              </a>
            )}
          </li>
        </ul>
      </div>
      <Modal
        open={showCreatePublicationModal}
        title="Create a new publication"
        className="govuk-!-width-one-half"
      >
        <fieldset className="govuk-fieldset">
          <div
            className="govuk-form-group govuk-!-margin-bottom-6"
            style={{ position: 'relative' }}
          >
            <h2 className="govuk-label-wrapper">
              <label className="govuk-label govuk-label--s" htmlFor="theme">
                Select a theme
              </label>
            </h2>
            <select className="govuk-select" id="theme">
              <option value="Children's social care">
                Children's social care
              </option>
              <option value="COVID-19">COVID-19</option>
              <option value="Destination of pupils and students">
                Destination of pupils and students
              </option>
              <option value="Early years">Early years</option>
              <option value="Finance and funding">Finance and funding</option>
              <option value="Further education">Further education</option>
              <option value="Higher education">Higher education</option>
              <option value="Pupils and schools">Pupils and schools</option>
              <option value="School and college outcomes and performance">
                School and college outcomes and performance
              </option>
              <option value="Teachers and school workforce">
                Teachers and school workforce
              </option>
              <option value="UK education and training statistics">
                UK education and training statistics
              </option>
            </select>
          </div>
        </fieldset>

        <Button
          onClick={() => {
            toggleCreatePublicationModal(false);
          }}
        >
          Create publication
        </Button>
        <Button
          className="govuk-!-margin-left-3"
          variant="secondary"
          onClick={() => {
            toggleCreatePublicationModal(false);
          }}
        >
          Cancel
        </Button>
      </Modal>
    </PrototypePage>
  );
};

export default PrototypeManageUsers;
