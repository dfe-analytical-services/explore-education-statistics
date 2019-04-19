import PrototypeDataSample
  from '@admin/pages/prototypes/publication/components/PrototypeDataSample';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import publicationService, { Release } from '@common/services/publicationService';
import React, { Component, Fragment } from 'react';
import {
  DragDropContext,
  Draggable,
  DraggableLocation,
  Droppable,
  DropResult,
} from 'react-beautiful-dnd';
import EditableAccordion from '../../components/EditableAccordion';
import EditableAccordionSection from '../../components/EditableAccordionSection';
import Link from '../../components/Link';
import EditableContentBlock from '../../modules/find-statistics/components/EditableContentBlock';
import { PrototypeEditableContent } from './components/PrototypeEditableContent';
import PrototypePage from './components/PrototypePage';

interface State {
  data: Release | undefined;
  publication: string | undefined;
  reordering: boolean;
}

const ACCORDION_ID_REGEXP = /accordion[(]([0-9]+)[)]/;

function getAccordionIndex(id: string | null | undefined) {
  if (id) {
    const regexpResult = ACCORDION_ID_REGEXP.exec(id);
    if (regexpResult) return regexpResult[1];
  }
  return null;
}

class PublicationPage extends Component<{}, State> {
  public onDragEnd = (result: DropResult) => {
    if (result.destination) {
      const { source } = result;
      const target: DraggableLocation = result.destination;

      if (result.type === 'accordion') {
        const sourceAccordion = getAccordionIndex(source.droppableId);
        const targetAccordion = getAccordionIndex(target.droppableId);

        if (sourceAccordion && targetAccordion) {
          if (source.index !== target.index) {
            if (this.state.data) {
              const data: Release = (this.state.data as unknown) as Release;

              const resultList = Array.from(data.content);
              const [moved] = resultList.splice(source.index, 1);
              resultList.splice(target.index, 0, moved);

              data.content = resultList;

              this.setState({ data });
            }
          }
        }
      }
    }
  };

  public state = {
    data: undefined,
    publication: undefined,
    reordering: false,
  };

  public async componentDidMount() {
    const publication = 'pupil-absence-in-schools-in-england';

    const request = publicationService.getLatestPublicationRelease(publication);

    const data = await request;

    this.setState({
      data,
      publication,
    });
  }

  public render() {
    if (this.state.data === undefined) return <div />;

    const data: Release = (this.state.data as unknown) as Release;

    return (
      <PrototypePage
        wide
        breadcrumbs={[
          {
            link: '/prototypes/admin-dashboard',
            text: 'Administrator dashboard',
          },
          { text: 'Edit pupil absence statistics', link: '#' },
        ]}
      >
        <div className="govuk-form-group">
          <fieldset className="govuk-fieldset">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--xl">
              <h1 className="govuk-fieldset__heading">Set page status</h1>
            </legend>
            <div className="govuk-radios govuk-radios--inline">
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  defaultChecked
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="edit"
                >
                  Editing in progress
                </label>
              </div>

              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="readyApproval"
                  value="edit"
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="readyApproval"
                >
                  Ready for approval
                </label>
              </div>

              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="cancelEdit"
                  value="edit"
                  data-aria-controls="content-for-approval"
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="cancelEdit"
                >
                  Cancel editing
                </label>
              </div>
            </div>
          </fieldset>
          <button className="govuk-button govuk-!-margin-top-3">
            Update page status
          </button>
        </div>

        <hr />
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <PrototypeEditableContent
              content={`
          <h1 className="govuk-heading-xl">
            Pupil absence statistics and data for schools in England
          </h1>
          `}
            />

            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">Published:</dt>
              <dd>
                <strong>22 March 2018</strong>
              </dd>
            </dl>

            <PrototypeEditableContent
              content={`
          <p className="govuk-body">
            Read national statistical summaries and definitions, view charts and
            tables and download data files across a range of pupil absence
            subject areas.
          </p>
          <p className="govuk-body">
            You can also view a regional breakdown of statistics and data within
            the
            <a href="#contents-sections-heading-9">
              <strong>local authorities section</strong>
            </a>
          </p>
          `}
            />

            <p>
              <Link to="/prototypes/methodology-absence">
                Find out more about our pupil absence data and statistics
                methodology and terminology
              </Link>
            </p>

            <Details summary="Download underlying data files">
              <ul className="govuk-list">
                <li>
                  <a href="#" className="govuk-link">
                    Download pdf files
                  </a>
                </li>
                <li>
                  <a href="#" className="govuk-link">
                    Download Excel files
                  </a>
                </li>
                <li>
                  <a href="#" className="govuk-link">
                    Download .csv files
                  </a>
                </li>
                <li>
                  <a href="#" className="govuk-link">
                    Access API
                  </a>
                </li>
              </ul>
            </Details>
          </div>

          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h2 className="govuk-heading-m" id="subsection-title">
                About these statistics
              </h2>

              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <span className="govuk-caption-m govuk-caption-inline">
                  For school year:{' '}
                </span>
                2016/17 (latest data)
              </h3>

              <Details summary="See previous 7 releases">
                <ul className="govuk-list">
                  <li>
                    <a
                      className="govuk-link"
                      href="/themes/schools/absence-and-exclusions/pupil-absence-in-schools-in-england/2015-16"
                    >
                      2015 to 2016
                    </a>
                  </li>
                  <li>
                    <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015">
                      2014 to 2015
                    </a>
                  </li>
                  <li>
                    <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014">
                      2013 to 2014
                    </a>
                  </li>
                  <li>
                    <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013">
                      2012 to 2013
                    </a>
                  </li>
                  <li>
                    <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics">
                      2011 to 2012
                    </a>
                  </li>
                  <li>
                    <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011">
                      2010 to 2011
                    </a>
                  </li>
                  <li>
                    <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010">
                      2009 to 2010
                    </a>
                  </li>
                </ul>
              </Details>

              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <span className="govuk-caption-m">Last updated: </span>20 June
                2018
              </h3>

              <Details summary="See all 2 updates">
                <div data-testid="publication-page--update-element">
                  <h3 className="govuk-heading-s">19 April 2017</h3>
                  <p>
                    Underlying data file updated to include absence data by
                    pupil residency and school location, andupdated metadata
                    document.
                  </p>
                </div>
                <div data-testid="publication-page--update-element">
                  <h3 className="govuk-heading-s">22 March 2017</h3>
                  <p>First published.</p>
                </div>
              </Details>

              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <span className="govuk-caption-m">Next update: </span>22 March
                2019
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-0">
                <a href="#">Notify me</a>
              </p>

              <h2
                className="govuk-heading-m govuk-!-margin-top-6"
                id="related-content"
              >
                Related content
              </h2>
              <nav role="navigation" aria-labelledby="related-content">
                <ul className="govuk-list">
                  <li>
                    <Link to="/prototypes/methodology-absence">
                      Pupil absence statistics: guidance and methodology
                    </Link>
                  </li>
                </ul>
              </nav>
            </aside>
          </div>
        </div>
        <hr />
        <h2 className="govuk-heading-l">
          Latest headline facts and figures - 2016/17
        </h2>
        <PrototypeDataSample
          sectionId="headlines"
          chartTitle="change in absence types in England"
          xAxisLabel="School Year"
          yAxisLabel="Absence Rate"
          chartData={[
            {
              authorised: 4.2,
              name: '2012/13',
              overall: 5.3,
              unauthorised: 1.1,
            },
            {
              authorised: 3.5,
              name: '2013/14',
              overall: 4.5,
              unauthorised: 1.1,
            },
            {
              authorised: 3.5,
              name: '2014/15',
              overall: 4.6,
              unauthorised: 1.1,
            },
            {
              authorised: 3.4,
              name: '2015/16',
              overall: 4.6,
              unauthorised: 1.1,
            },
            {
              authorised: 3.4,
              name: '2016/17',
              overall: 4.7,
              unauthorised: 1.3,
            },
          ]}
          chartDataKeys={['unauthorised', 'authorised', 'overall']}
        />

        {data.content.length > 0 ? (
          this.state.reordering ? (
            <Fragment>
              <DragDropContext onDragEnd={this.onDragEnd}>
                <h2 className="govuk-heading-l reorderable-relative">
                  <button
                    className="reorderable"
                    onClick={() => this.setState({ reordering: false })}
                  >
                    Done
                  </button>
                  Contents
                </h2>

                <div className="govuk-accordion__controls">&nbsp;</div>

                <Droppable droppableId="accordion(0)" type="accordion">
                  {droppableProvided => (
                    <div
                      {...droppableProvided.droppableProps}
                      ref={droppableProvided.innerRef}
                    >
                      {data.content.map(({ heading }, index) => (
                        <Draggable
                          draggableId={`section(${index})`}
                          index={index}
                          key={`${heading}_${index}`}
                        >
                          {draggableProvided => (
                            <div
                              className="govuk-accordion__section"
                              ref={draggableProvided.innerRef}
                              {...draggableProvided.draggableProps}
                            >
                              <div className="govuk-accordion__section-header">
                                <h2 className="govuk-accordion__section-heading reorderable-relative">
                                  <span
                                    className="drag-handle"
                                    {...draggableProvided.dragHandleProps}
                                  />
                                  <button className="govuk-accordion__section-button">
                                    {heading}
                                  </button>
                                </h2>
                              </div>
                            </div>
                          )}
                        </Draggable>
                      ))}

                      {droppableProvided.placeholder}
                    </div>
                  )}
                </Droppable>
              </DragDropContext>
            </Fragment>
          ) : (
            <div>
              <h2 className="govuk-heading-l reorderable-relative">
                <button
                  className="reorderable"
                  onClick={() => this.setState({ reordering: true })}
                >
                  Reorder sections
                </button>
                Contents
              </h2>

              <EditableAccordion id="contents-sections" index={0}>
                {data.content.map(
                  ({ heading, caption, order, content }, index) => (
                    <EditableAccordionSection
                      heading={heading}
                      caption={caption}
                      index={index}
                      key={`${order}_${index}`}
                    >
                      <EditableContentBlock
                        content={content}
                        id={`editable-block-${index}`}
                      />
                    </EditableAccordionSection>
                  ),
                )}
              </EditableAccordion>
            </div>
          )
        ) : (
          ''
        )}

        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Extra information
        </h2>
        <Accordion id="extra-information-sections">
          <AccordionSection
            heading="Where does this data come from?"
            caption="Our methodology, how we collect and process the data"
            headingTag="h3"
          >
            <ul className="govuk-list">
              <li>
                <a
                  href="/prototypes/methodology-absence"
                  className="govuk-link"
                >
                  How do we collect it?
                </a>
              </li>
              <li>
                <a
                  href="/prototypes/methodology-absence"
                  className="govuk-link"
                >
                  What do we do with it?
                </a>
              </li>
              <li>
                <a
                  href="/prototypes/methodology-absence"
                  className="govuk-link"
                >
                  Related policies
                </a>
              </li>
            </ul>
          </AccordionSection>
          <AccordionSection heading="Feedback and questions" headingTag="h3">
            <ul className="govuk-list">
              <li>
                <a href="#" className="govuk-link">
                  Feedback on this page
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Make a suggestion
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Ask a question
                </a>
              </li>
            </ul>
          </AccordionSection>
          <AccordionSection heading="Contact us" headingTag="h3">
            <h4 className="govuk-heading-">Media enquiries</h4>
            <address className="govuk-body dfe-font-style-normal">
              Press Office News Desk
              <br />
              Department for Education <br />
              Sanctuary Buildings <br />
              Great Smith Street <br />
              London
              <br />
              SW1P 3BT <br />
              Telephone: 020 7783 8300
            </address>

            <h4 className="govuk-heading-">Other enquiries</h4>
            <address className="govuk-body dfe-font-style-normal">
              Data Insight and Statistics Division
              <br />
              Level 1<br />
              Department for Education
              <br />
              Sanctuary Buildings <br />
              Great Smith Street
              <br />
              London
              <br />
              SW1P 3BT <br />
              Telephone: 020 7783 8300
              <br />
              Email: <a href="#">Schools.statistics@education.gov.uk</a>
            </address>
          </AccordionSection>
        </Accordion>

        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Exploring the data
        </h2>
        <p>
          The statistics can be viewed as reports, or you can customise and
          download as excel or .csv files . The data can also be accessed via an
          API. <a href="#">What is an API?</a>
        </p>
        <Link to="/prototypes/table-tool" className="govuk-button">
          Create charts and tables
        </Link>
        <div className="govuk-!-margin-top-9">
          <a href="#print" className="govuk-link">
            Print this page
          </a>
        </div>
      </PrototypePage>
    );
  }
}

export default PublicationPage;
