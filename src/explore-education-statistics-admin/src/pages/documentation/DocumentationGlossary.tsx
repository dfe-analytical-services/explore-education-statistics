import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Page from '@admin/components/Page';
import React from 'react';

const DocumentationGlossary = () => {
  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Glossary' },
      ]}
      title="Glossary"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Style guidance</span>
            <h1 className="govuk-heading-xl">Service glossary</h1>
          </div>
          <p>
            Browse our A to Z list of style, spelling and grammar conventions
            for all content published on the Explore education statistics
            service.
          </p>

          <Accordion id="a-z">
            <AccordionSection heading="A">
              <h3 id="a42-a42s">A*, A*s</h3>
              <p>
                The top ABC grade in GCSEs and A levels. Use the symbol * not
                the word ‘star’. No apostrophe in the plural.
              </p>
              <h3 id="a-level">A level</h3>
              <p>No hyphen. Lower case level.</p>
              <h3 id="abbreviations-and-acronyms">
                Abbreviations and acronyms
              </h3>
              <p>
                The first time you use an abbreviation or acronym explain it in
                full on each page unless it’s well known, like UK, DVLA, US, EU,
                VAT and MP. This includes government departments or schemes.
                Then refer to it by initials, and use{' '}
                <a href="https://www.gov.uk/guidance/how-to-publish-on-gov-uk/markdown#acronyms">
                  acronym Markdown
                </a>{' '}
                so the full explanation is available as hover text.
              </p>
              <p>
                If you think an acronym is well known, please provide evidence
                that 80% of the UK population will understand and commonly use
                it. Evidence can be from search analytics or testing of a
                representative sample.
              </p>
              <p>Do not use full stops in abbreviations: BBC, not B.B.C.</p>
              <h3 id="the-academies-programme">the academies programme</h3>
              <p>Lower case.</p>
              <h3 id="academy">academy</h3>
              <p>
                Only use upper case when referring to the name of an academy,
                like Mossbourne Community Academy. See also{' '}
                <a href="#titles">Titles</a>.
              </p>
              <h3 id="academy-converters">academy converters</h3>
              <p>Lower case.</p>
              <h3 id="academy-order">academy order</h3>
              <p>Lower case.</p>
              <h3 id="academy-trust">academy trust</h3>
              <p>Lower case.</p>
              <h3 id="act-act-of-parliament">act, act of Parliament</h3>
              <p>
                Lower case. Only use upper case when using the full title:
                Planning and Compulsory Purchase Act 2004, for example.
              </p>
              <h3 id="active-voice">Active voice</h3>
              <p>
                Use the active rather than passive voice. This will help us
                write concise, clear content.
              </p>
              <h3 id="addressing-the-user">Addressing the user</h3>
              <p>
                Address the user as ‘you’ where possible. Content on the site
                often makes a direct appeal to citizens and businesses to get
                involved or take action: ‘You can contact HMRC by phone and
                email’ or ‘Pay your car tax’, for example.
              </p>
              <h3 id="admin-champions">Admin Champions</h3>
              <p>
                Name of the group of DfE Statistics staff helping to develop and
                ‘champion’ the functions and processes of the Admin system
                across DfE Statistics.
              </p>
              <p>
                See also <a href="#admin-system">Admin system</a>.
              </p>
              <h3 id="admin-system">Admin system</h3>
              <p>
                Term describing the publishing system used to manage data and
                content and publish the latest ‘releases’ of statistics and data
                on Explore education statistics.
              </p>
              <h3 id="ages">ages</h3>
              <p>
                Do not use hyphens in ages unless to avoid confusion, although
                it’s always best to write in a way that avoids ambiguity. For
                example, ‘a class of 15 16-year-old students took the A level
                course’ can be written as ‘15 students aged 16 took the A level
                course’.
              </p>
              <h3 id="ampersand">Ampersand</h3>
              <p>
                Use 'and' rather than &amp;, unless it’s a department’s logo
                image or a company’s name as it appears on the{' '}
                <a rel="external" href="https://beta.companieshouse.gov.uk/">
                  Companies House
                </a>{' '}
                register.
              </p>
              <h3 id="analysts">analysts</h3>
              <p>Users who use but don’t produce statistics.</p>
              <h3 id="api">API (or APIs)</h3>
              <p>
                Technical applications or software which allow external or
                internal applications, services or systems to access the data or
                features of another application, service or system.
              </p>
              <p>
                For example, a data API for Explore education statistics would
                allow other applications, services or systems outside of our
                service to access the data used Explore education statistics.
              </p>
              <p>
                See also{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="applied-general-qualifications">
                applied general qualifications
              </h3>
              <p>Lower case.</p>
              <h3 id="apprenticeship-programme">apprenticeship programme</h3>
              <p>Lower case.</p>
              <h3 id="approved-for-release">Approved for release</h3>
              <p>
                Term describing the release status of a release once it’s been
                signed-off by a Responsible Statistician.
              </p>
              <p>
                See also <a href="#release-status">Release status</a>.
              </p>
              <p>
                See also{' '}
                <a href="#responsible-statistician">Responsible Statistician</a>
                .
              </p>
              <h3 id="author">Author</h3>
              <p>
                See <a href="#primary-analyst">Primary Analyst</a>.
              </p>
            </AccordionSection>
            <AccordionSection heading="B">
              <h3 id="banned-words">Banned words</h3>
              <p>
                See <a href="#words-to-avoid">Words to avoid</a>
              </p>
              <h3 id="baseline">baseline</h3>
              <p>One word, lower case.</p>
              <h3 id="bold">bold</h3>
              <p>
                Only use bold to refer to text from interfaces in technical
                documentation or instructions.
              </p>
              <p>
                You can use bold to explain what field a user needs to fill in
                on a form, or what button they need to select. For example:
                “Select <strong>Continue</strong>. The{' '}
                <strong>Verify Certificate</strong> window opens.”
              </p>
              <p>
                Use bold sparingly - using too much will make it difficult for
                users to know which parts of your content they need to pay the
                most attention to.
              </p>
              <p>
                Do not use bold in other situations, for example to emphasise
                text.
              </p>
              <p>To emphasise words or phrases, you can:</p>
              <ul>
                <li>front-load sentences</li>
                <li>use headings</li>
                <li>
                  use{' '}
                  <a href="/guidance/style-guide/a-to-z-of-gov-uk-style#bullet-points-steps">
                    bullets
                  </a>
                </li>
              </ul>
              <h3 id="borough-council">borough council</h3>
              <p>Lower case even in a name: Northampton borough council.</p>
              <h3 id="brackets">Brackets</h3>
              <p>
                Use (round brackets), not [square brackets]. The only acceptable
                use of square brackets is for explanatory notes in reported
                speech:
              </p>
              <p>“Thank you [Foreign Minister] Mr Smith.”</p>
              <p>
                Do not use round brackets to refer to something that could
                either be singular or plural, like ‘Check which document(s) you
                need to send to DVLA.’
              </p>
              <p>
                Always use the plural instead, as this will cover each
                possibility: ‘Check which documents you need to send to DVLA.’
              </p>
              <h3 id="britain">Britain</h3>
              <p>
                See <a href="#great-britain">Great Britain</a>
              </p>
              <h3 id="btec-national-diploma">BTEC National Diploma</h3>
              <p>Upper case.</p>
              <h3 id="bullet-points-steps">Bullet points and steps</h3>
              <p>
                You can use bullet points to make text easier to read. Make sure
                that:
              </p>
              <ul>
                <li>you always use a lead-in line</li>
                <li>the bullets make sense running on from the lead-in line</li>
                <li>you use lower case at the start of the bullet</li>
                <li>
                  you do not use more than one sentence per bullet point - use
                  commas or dashes to expand on an item
                </li>
                <li>you do not put ‘or’ or ‘and’ after the bullets</li>
                <li>
                  if you add links they appear within the text and not as the
                  whole bullet
                </li>
                <li>you do not put a semicolon at the end of a bullet</li>
                <li>there is no full stop after the last bullet point</li>
              </ul>
              <p>
                Bullets should normally form a complete sentence following from
                the lead text. But it’s sometimes necessary to add a short
                phrase to clarify whether all or some of the points apply. For
                example, ‘You can only register a pension scheme that is (one of
                the following):’
              </p>
              <h4 id="steps">Steps</h4>
              <p>
                Use numbered steps instead of bullet points to guide a user
                through a process. You do not need a lead-in line and you can
                use links and downloads (with{' '}
                <a href="https://www.gov.uk/guidance/how-to-publish-on-gov-uk/markdown#numbered-list">
                  appropriate Markdown
                </a>
                ) in steps. Steps end in a full stop because each should be a
                complete sentence.
              </p>
            </AccordionSection>
            <AccordionSection heading="C">
              <h3 id="c-of-e">C of E</h3>
              <p>For Church of England when referring to school names.</p>
              <h3 id="capitalisation">Capitalisation</h3>
              <p>
                DO NOT USE BLOCK CAPITALS FOR LARGE AMOUNTS OF TEXT AS IT’S
                QUITE HARD TO READ.
              </p>
              <p>
                Always use lower case, even in page titles. The exceptions to
                this are proper nouns, and:
              </p>
              <ul>
                <li>
                  departments (specific government departments - see below)
                </li>
                <li>the Civil Service, with lower case for ‘the’</li>
                <li>
                  job titles, ministers’ role titles: Minister for Housing, Home
                  Secretary
                </li>
                <li>
                  titles like Mr, Mrs, Dr, the Duke of Cambridge (the duke at
                  second mention); Pope Francis, but the pope
                </li>
                <li>Rt Hon (no full stops)</li>
                <li>buildings</li>
                <li>place names</li>
                <li>brand names</li>
                <li>faculties, departments, institutes and schools</li>
                <li>
                  names of groups, directorates and organisations: Knowledge and
                  Innovation Group
                </li>
                <li>Parliament, the House</li>
                <li>
                  titles of specific acts or bills: Housing Reform Bill (but use
                  ‘the act’ or ‘the bill’ after the first time you use the full
                  act or bill title)
                </li>
                <li>
                  names of specific, named government schemes known to people
                  outside government: Right to Buy, Queen’s Awards for
                  Enterprise
                </li>
                <li>
                  specific select committees: Public Administration Select
                  Committee
                </li>
                <li>header cells in tables: Annual profits</li>
                <li>
                  titles of books (and within single quotes), for example, ‘The
                  Study Skills Handbook’
                </li>
                <li>World War 1 and World War 2 (note caps and numbers)</li>
              </ul>
              <p>Do not capitalise:</p>
              <ul>
                <li>
                  government - see{' '}
                  <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#government">
                    government
                  </a>
                </li>
                <li>
                  minister, never Minister, unless part of a specific job title,
                  like Minister for the Cabinet Office
                </li>
                <li>
                  department or ministry - never Department or Ministry, unless
                  referring to a specific one: Ministry of Justice, for example
                </li>
                <li>
                  white paper, green paper, command paper, House of Commons
                  paper
                </li>
                <li>
                  budget, autumn statement, spring statement, unless referring
                  to and using the full name of a specific statement - for
                  example, “2016 Budget”
                </li>
                <li>
                  sections or schedules within specific named acts, regulations
                  or orders
                </li>
                <li>
                  director general (no hyphen), deputy director, director,
                  unless in a specific job title
                </li>
                <li>
                  group and directorate, unless referring to a specific group or
                  directorate: the Commercial Directorate, for example
                </li>
                <li>departmental board, executive board, the board</li>
                <li>
                  policy themes like sustainable communities, promoting economic
                  growth, local enterprise zones
                </li>
                <li>
                  general mention of select committees (but do cap specific ones
                  - see above)
                </li>
                <li>the military</li>
              </ul>
              <h4 id="capitals-for-government-departments">
                Capitals for government departments
              </h4>
              <p>
                Use the following conventions for government departments. A
                department using an ampersand in its logo image is fine but use
                ‘and’ when writing in full text.
              </p>
              <ul>
                <li>Attorney General’s Office (AGO)</li>
                <li>Cabinet Office (CO)</li>
                <li>
                  Department for Business, Energy and Industrial Strategy (BEIS)
                </li>
                <li>Department for Digital, Culture, Media and Sport (DCMS)</li>
                <li>Department for Education (DfE)</li>
                <li>
                  Department for Environment, Food and Rural Affairs (Defra)
                </li>
                <li>Department for Exiting the European Union (DExEU)</li>
                <li>Department for International Development (DFID)</li>
                <li>Department for International Trade (DIT)</li>
                <li>Department for Transport (DfT)</li>
                <li>Department for Work and Pensions (DWP)</li>
                <li>Department of Health and Social Care (DHSC)</li>
                <li>Foreign and Commonwealth Office (FCO)</li>
                <li>HM Treasury (HMT)</li>
                <li>Home Office (HO)</li>
                <li>Ministry of Defence (MOD)</li>
                <li>
                  Ministry of Housing, Communities and Local Government (MHCLG)
                </li>
                <li>Ministry of Justice (MOJ)</li>
              </ul>
              <h3 id="care-worker">care worker</h3>
              <p>Two words. Lower case.</p>
              <h3 id="chair-of-governors">chair of governors</h3>
              <p>Lower case.</p>
              <h3 id="chairman-chairwoman-chairperson">
                chairman, chairwoman, chairperson
              </h3>
              <p>
                Lower case in text. Upper case in titles: Spencer Tracy,
                Chairman, GDS.
              </p>
              <h3 id="charts">Charts</h3>
              <p>
                Term referring to the chart functionality available within the
                Admin system and on Explore education statistics.
              </p>
              <h3 id="childcare">childcare</h3>
              <p>Lower case.</p>
              <h3 id="children-in-need">Children in Need</h3>
              <p>
                Upper case for the BBC fundraising event, lower case for
                children in need census.
              </p>
              <h3 id="civil-service">Civil Service</h3>
              <p>Upper case.</p>
              <h3 id="civil-servants">civil servants</h3>
              <p>Lower case.</p>
              <h3 id="classwork">classwork</h3>
              <p>One word.</p>
              <h3 id="click">click</h3>
              <p>
                Don’t use “click” when talking about user interfaces because not
                all users click. Use “select”.
              </p>
              <h3 id="code-of-practice">code of practice</h3>
              <p>Lower case.</p>
              <h3 id="community-voluntary-and-foundation-schools">
                community, voluntary and foundation schools
              </h3>
              <p>Lower case.</p>
              <h3 id="content-design-champions">Content Design Champions</h3>
              <p>
                Name of the group of DfE Statistics staff helping to develop and
                ‘champion’ the creation of good quality content across DfE
                Statistics so users can quickly and easily understand the
                statistical commentary on Explore education statistics.
              </p>
              <h3 id="contractions">Contractions</h3>
              <p>Use contractions like you’re and we’ll.</p>
              <p>Avoid:</p>
              <ul>
                <li>
                  negative contractions like can’t and don’t - many users find
                  them harder to read, or misread them as the opposite of what
                  they say
                </li>
                <li>
                  should’ve, could’ve, would’ve, they’ve - these can be hard to
                  read
                </li>
              </ul>
              <p>
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#contractions">
                  Read more about contractions
                </a>
                .
              </p>
              <h3 id="co-operation">co-operation</h3>
              <p>Hyphenated.</p>
              <h3 id="core-standards">core standards</h3>
              <p>Lower case.</p>
              <h3 id="council">council</h3>
              <p>Lower case even in a name: Wandsworth council.</p>
              <h3 id="council-tax">Council Tax</h3>
              <p>Upper case.</p>
              <h3 id="countries-and-territories">countries and territories</h3>
              <p>
                When referring to a country or territory, use the names listed
                in the{' '}
                <a rel="external" href="https://country.register.gov.uk/">
                  country register
                </a>{' '}
                or{' '}
                <a rel="external" href="https://territory.register.gov.uk/">
                  territory register
                </a>
                .
              </p>
              <h3 id="coursework">coursework</h3>
              <p>One word.</p>
              <h3 id="cross-curricular-learning">cross-curricular learning</h3>
              <p>Hyphenated.</p>
              <h3 id="curriculums">curriculums</h3>
              <p>Not curricula.</p>
            </AccordionSection>
            <AccordionSection heading="D">
              <h3 id="data">data</h3>
              <p>
                Term describing the machine readable data files uploaded to the
                service which drive the table and chart tools and can be
                downloaded by users.
              </p>
              <h3 id="data-blocks">Data blocks</h3>
              <p>
                Term referring to the functionality which filters the data used
                to create tables and charts within the Admin system and on
                Explore education statistics.
              </p>
              <h3 id="data-centre">data centre</h3>
              <p>Not “datacentre”.</p>
              <h3 id="data-champions">Data Champions</h3>
              <p>
                Name of the group of DfE Statistics staff helping to develop and
                ‘champion’ the correct use of data across DfE Statistics and
                produce consistent machine readable data files for explore
                education statistics and its users.
              </p>
              <h3 id="data-dissemination-project">
                Data Dissemination Project
              </h3>
              <p>
                Alternative internal name for the project before explore
                education statistics.
              </p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="data-set">data set</h3>
              <p>Not “dataset”.</p>
              <h3 id="data-store">data store</h3>
              <p>Not “datastore”.</p>
              <h3 id="dates">Dates</h3>
              <ul>
                <li>use upper case for months: January, February</li>
                <li>
                  do not use a comma between the month and year: 4 June 2017
                </li>
                <li>
                  when space is an issue - in tables or publication titles, for
                  example - you can use truncated months: Jan, Feb
                </li>
                <li>
                  we use ‘to’ in date ranges - not hyphens, en rules or em
                  dashes. For example:
                  <ul>
                    <li>tax year 2011 to 2012</li>
                    <li>
                      Monday to Friday, 9am to 5pm (put different days on a new
                      line, do not separate with a comma)
                    </li>
                    <li>10 November to 21 December</li>
                  </ul>
                </li>
                <li>
                  do not use quarter for dates, use the months: ‘department
                  expenses, Jan to Mar 2013’
                </li>
                <li>
                  when referring to today (as in a news article) include the
                  date: ‘The minister announced today (14 June 2012) that…’
                </li>
              </ul>
              <p>
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#date-ranges">
                  Read more about dates
                </a>
                .
              </p>
              <h3 id="daycare-trust">Daycare Trust</h3>
              <p>Two words. Upper case.</p>
              <h3 id="dedicated-schools-grant">dedicated schools grant</h3>
              <p>Lower case.</p>
              <h3 id="department">department</h3>
              <p>
                Lower case except when in the title: the Department of Health
                and Social Care.
              </p>
              <h3 id="dfe-data-dissemination-project">
                DFE Data Dissemination Project
              </h3>
              <p>
                Official internal name for the project building explore
                education statistics.
              </p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="diploma">diploma</h3>
              <p>
                Lower case unless part of a title like Edexcel L2 Diploma in IT.
              </p>
              <h3 id="director">director</h3>
              <p>
                Lower case in text. Upper case in titles: Spencer Tracy,
                Director, GDS.
              </p>
              <h3 id="director-general">director general</h3>
              <p>Lower case. No hyphen.</p>
              <h3 id="district-council">district council</h3>
              <p>Lower case even in a name, like Warwick district council.</p>
              <h3 id="dissemination-platform">Dissemination platform</h3>
              <p>Alternative internal name for Explore education statistics.</p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="dissemination-tool">Dissemination tool</h3>
              <p>Alternative internal name for Explore education statistics.</p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
            </AccordionSection>
            <AccordionSection heading="E">
              <h3 id="early-years">early years</h3>
              <p>Lower case.</p>
              <h3 id="early-years-foundation-stage-eyfs">
                early years foundation stage (EYFS)
              </h3>
              <p>Lower case.</p>
              <h3 id="early-years-professional-status">
                early years professional status
              </h3>
              <p>Lower case.</p>
              <h3 id="early-years-teacher">early years teacher</h3>
              <p>Lower case.</p>
              <h3 id="early-years-teacher-status">
                early years teacher status
              </h3>
              <p>Lower case.</p>
              <h3 id="ebacc">EBacc</h3>
              <p>A performance measure linked to GCSEs. Upper case E and B.</p>
              <h3 id="eco-schools">eco-schools</h3>
              <p>Hyphenated.</p>
              <h3 id="eg-etc-and-ie">eg, etc and ie</h3>
              <p>
                eg can sometimes be read aloud as ‘egg’ by screen reading
                software. Instead use ‘for example’ or ‘such as’ or ‘like’ or
                ‘including’ - whichever works best in the specific context.
              </p>
              <p>
                etc can usually be avoided. Try using ‘for example’ or ‘such as’
                or ‘like’ or ‘including’. Never use etc at the end of a list
                starting with these words.
              </p>
              <p>
                ie - used to clarify a sentence - is not always well understood.
                Try (re)writing sentences to avoid the need to use it. If that
                is not possible, use an alternative such as ‘meaning’ or ‘that
                is’.
              </p>
              <h3 id="email">email</h3>
              <p>One word.</p>
              <h3 id="email-addresses">Email addresses</h3>
              <p>
                Write email addresses in full, in lower case and as active
                links. Do not include any other words in the link text.
              </p>
              <h3 id="enrol">enrol</h3>
              <p>Lower case.</p>
              <h3 id="enrolling">enrolling</h3>
              <p>Lower case.</p>
              <h3 id="enrolment">enrolment</h3>
              <p>Lower case.</p>
              <h3 id="european-commission">European Commission</h3>
              <p>
                Leave unabbreviated to distinguish from the European Community.
                Write out in full at first mention, then call it the Commission.
              </p>
              <h3 id="european-union-vs-european-community">
                European Union vs European Community
              </h3>
              <p>
                Use EU when you mean EU member states: EU countries, EU
                businesses, EU consumers, goods exported from the EU, EU VAT
                numbers.
              </p>
              <p>EC should be used when it’s EC directives, EC Sales List.</p>
              <h3 id="euros-the-euro">euros, the euro</h3>
              <p>Lower case.</p>
              <h3 id="etc">etc</h3>
              <p>
                See <a href="#eg-etc-ie">eg, etc and ie</a>
              </p>
              <h3 id="excel-spreadsheet">Excel spreadsheet</h3>
              <p>Upper case because Excel is a brand name.</p>
              <h3 id="executive-director">executive director</h3>
              <p>
                Lower case in text. Upper case in titles: Spencer Tracy,
                Executive Director, GDS.
              </p>
              <h3 id="explore-education-statistics">
                Explore education statistics
              </h3>
              <p>
                Official GDS styled name (hence the lower case format) for the
                Explore education statistics service which is used and displayed
                on the live online service.
              </p>
              <p>
                Should be used to refer to the service in wider GOV.UK and
                governmental circles.
              </p>
              <h3 id="explore-education-statistics-service">
                Explore education statistics service
              </h3>
              <p>
                Alternative full internal internal name for explore education
                statistics.
              </p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>

              <h3 id="extra-curricular">extra-curricular</h3>
              <p>Hyphenated</p>
            </AccordionSection>
            <AccordionSection heading="F">
              <h3 id="faqs-frequently-asked-questions">
                FAQs (frequently asked questions)
              </h3>
              <p>
                Do not use FAQs. If you write content by starting with user
                needs, you will not need to use FAQs.
              </p>
              <p>
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#dont-use-faqs">
                  Read more about FAQs
                </a>
                .
              </p>
              <h3 id="finance-and-procurement">finance and procurement</h3>
              <p>Lower case.</p>
              <h3 id="fine">fine</h3>
              <p>Use ‘fine’ instead of ‘financial penalty’.</p>
              <p>For example, “You’ll pay a £50 fine.”</p>
              <p>
                For other types of sanction, say what will happen to the user -
                you’ll get points on your licence, go to court and so on. Only
                say ‘civil penalty’ if there’s evidence users are searching for
                the term.
              </p>
              <p>
                Describe what the user might need to do, rather than what
                government calls a thing.
              </p>
              <h3 id="fixed-period-exclusions">fixed-period exclusions</h3>
              <p>Hyphenated.</p>
              <h3 id="footnotes">Footnotes</h3>
              <p>
                Term referring to the functionality which adds technical
                footnotes to data within the Admin system and then dynamically
                displays them for users on Explore education statistics when
                they use the table tool.
              </p>
              <p>
                See also
                <a href="#release-footnotes">Release footnotes</a>.
              </p>
              <h3 id="foundation-degrees">foundation degrees</h3>
              <p>Lower case.</p>
              <h3 id="foundation-schools">foundation schools</h3>
              <p>Lower case.</p>
              <h3 id="foundation-stage--foundation-subjects">
                foundation stage / foundation subjects
              </h3>
              <p>Lower case.</p>
              <h3 id="foundation-trust">foundation trust</h3>
              <p>
                Lower case unless the full name of the foundation trust is being
                used: Salisbury NHS Foundation Trust.
              </p>
              <h3 id="free-school">free school</h3>
              <p>Lower case.</p>
              <h3 id="the-free-schools-programme">
                the free schools programme
              </h3>
              <p>Lower case.</p>
              <h3 id="free-school-meals">free school meals</h3>
              <p>Lower case.</p>
              <h3 id="freedom-of-information">Freedom of Information</h3>
              <p>
                You can make a Freedom of Information (FOI) request, but not a
                request under the FOI Act.
              </p>
              <h3 id="funding-agreement">funding agreement</h3>
              <p>Lower case.</p>
              <h3 id="further-education-fe">further education (FE)</h3>
              <p>Lower case.</p>
            </AccordionSection>
            <AccordionSection heading="G">
              <h3 id="gcse-gcses">GCSE, GCSEs</h3>
              <p>
                No full stops between the initials. No apostrophe in the plural.
              </p>
              <h3 id="gds">GDS</h3>
              <p>
                See{' '}
                <a href="#government-digital-service-gds">
                  Government Digital Service (GDS)
                </a>
                .
              </p>
              <h3 id="gds-assessment">GDS assessment</h3>
              <p>
                All government and public facing digital transactional services
                (including Explore education statistics) must pass an assessment
                run by GDS in order to finally go live to the public and become
                an ‘official’ online government service.
              </p>
              <ul>
                <p>
                  Services are assessed at each of the following phases of
                  development:
                </p>
                <li>at the end of alpha</li>
                <li>
                  at the end of private beta - before its public beta launch
                </li>
                <li>at the end of beta - before it goes live</li>
              </ul>
              <p>
                If a service doesn’t pass an assessment, it will need to get
                reassessed before it can start the next stage.
              </p>
              <h3 id="general-election">general election</h3>
              <p>Lower case.</p>
              <h3 id="geography-and-regions">Geography and regions</h3>
              <p>
                Use lower case for north, south, east and west, except when
                they’re part of a name or recognised region.
              </p>
              <p>
                So, the south-west (compass direction), but the South West
                (administrative region).
              </p>
              <p>
                Use lower case for the north, the south of England, the
                south-west, north-east Scotland, south Wales, the west, western
                Europe, the far east, south-east Asia.
              </p>
              <p>
                Use upper case for East End, West End (London), Middle East,
                Central America, South America, Latin America.
              </p>
              <p>
                Always write out the full name of the area the first time you
                use it. You can use a capital for a shortened version of a
                specific area or region if it’s commonly known by that name,
                like the Pole for the North Pole.
              </p>
              <h3 id="governing-body">governing body</h3>
              <p>Singular noun.</p>
              <p>
                The governing body is meeting today. It will decide who to
                appoint.
              </p>
              <h3 id="government">government</h3>
              <p>
                Lower case unless it’s a full title. For example: ‘UK
                government’, but ‘Her Majesty’s Government of the United Kingdom
                of Great Britain and Northern Ireland’.
              </p>
              <p>Also ‘Welsh Government’, as it’s the full title.</p>
              <h3 id="government-offices">government offices</h3>
              <p>Lower case.</p>
              <h3 id="governor">governor</h3>
              <p>Lower case.</p>
              <h3 id="govuk">GOV.UK</h3>
              <p>
                Domain name and term referring to the UK government website for
                finding government services and information which is maintained
                by the Government Digital Service (GDS).
              </p>
              <p>Also, widely used as an alternative name to refer to GDS.</p>
              <p>
                See{' '}
                <a href="#government-digital-service-gds">
                  Government Digital Service (GDS)
                </a>
                .
              </p>
              <h3 id="dotgov">.GOV (ie dotGOV)</h3>
              <p>
                Term referring to the domain name and to refer to the UK
                government website for finding government services and
                information which is maintained by the Government Digital
                Service (GDS).
              </p>
              <p>
                See also{' '}
                <a href="#government-digital-service-gds">
                  Government Digital Service (GDS)
                </a>
                .
              </p>
              <h3 id="government-digital-service">
                Government Digital Service (GDS)
              </h3>
              <p>
                Government department leading digital transformation across UK
                government and responsible for the digital standards which the
                DfE Data Dissemination Project and Explore education statistics
                has to meet.
              </p>
              <p>
                See also{' '}
                <a href="#dfe-data-dissemination-project">
                  DfE Data Dissemination Project
                </a>
                .
              </p>
              <p>
                See also{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="grammar-school">grammar school</h3>
              <p>
                Lower case unless part of a school name: The Manchester Grammar
                School.
              </p>
              <h3 id="great-britain">Great Britain</h3>
              <p>
                Refers only to England, Scotland and Wales excluding Northern
                Ireland.
              </p>
              <p>
                If you’re telling users about multiple areas, use (for example)
                ‘England, Scotland and Wales’.
              </p>
              <h4 id="britain-1">Britain</h4>
              <p>
                Use UK and United Kingdom in preference to Britain and British
                (UK business, UK foreign policy, ambassador and high
                commissioner). But British embassy, not UK embassy.
              </p>
              <h3 id="green-paper">green paper</h3>
              <p>Lower case.</p>
              <h3 id="group">Group</h3>
              <p>
                Upper case for names of groups, directorates and organisations:
                Knowledge and Innovation Group.
              </p>
              <p>
                Lower case when a group has a very generic title like working
                group or research team.
              </p>
              <h3 id="guidance">guidance</h3>
              <p>Lower case: national recovery guidance.</p>
              <h3 id="gypsies">Gypsies</h3>
              <p>
                Upper case because Gypsies are recognised as an ethnic group
                under the Race Relations Act.
              </p>
            </AccordionSection>
            <AccordionSection heading="H">
              <h3 id="headteacher">headteacher</h3>
              <p>One word. You can use head if the context is clear.</p>
              <h3 id="health-protection-unit">health protection unit</h3>
              <p>
                Lower case unless it’s the title of an organisation: North East
                and Central London Health Protection Unit.
              </p>
              <h3 id="helpdesk">helpdesk</h3>
              <p>Not “help desk”.</p>
              <h3 id="high-attaining-pupils">high-attaining pupils</h3>
              <p>Hyphenated.</p>
              <h3 id="higher-education-he">higher education (HE)</h3>
              <p>Lower case.</p>
              <h3>higher review</h3>
              <ul className="govuk-list govuk-list--bullet">
                <p>
                  Generic term describing the release status of a release once
                  it’s been reviewed, approved for sign-off and its release
                  status updated to one of the following:
                </p>
                <li>Ready for sign-off</li>
                <li>Approved for release</li>
                <li>In pre-release</li>
              </ul>
              <p>
                See also{' '}
                <a href="#approved-for-release">Approved for release</a>.
              </p>
              <p>
                See also <a href="#in-pre-release">In pre-release</a> .
              </p>
              <p>
                See also <a href="#ready-for-sign-off">Ready for sign-off</a> .
              </p>
              <h3 id="home-school-agreement">home-school agreement</h3>
              <p>Hyphenated.</p>
              <h3 id="hyphenation">Hyphenation</h3>
              <p>Hyphenate:</p>
              <ul>
                <li>re- words starting with e, like re-evaluate</li>
                <li>co-ordinate</li>
                <li>co-operate</li>
              </ul>
              <p>Do not hyphenate:</p>
              <ul>
                <li>reuse</li>
                <li>reinvent</li>
                <li>reorder</li>
                <li>reopen</li>
                <li>email</li>
              </ul>
              <p>
                Do not use a hyphen unless it’s confusing without it, for
                example, a little used-car is different from a little-used car.
                You can also refer to{' '}
                <a
                  rel="external"
                  href="https://www.theguardian.com/guardian-observer-style-guide-h"
                >
                  The Guardian style guide{' '}
                </a>{' '}
                for advice on hyphenation.
              </p>
              <p>
                Use ‘to’ for{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#times">
                  time
                </a>{' '}
                and{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#dates">
                  date ranges
                </a>
                , not hyphens.
              </p>
            </AccordionSection>
            <AccordionSection heading="I">
              <h3 id="ie">ie</h3>
              <p>
                See <a href="#eg-etc-and-ie">eg, etc and ie</a>
              </p>
              <h3 id="in-draft">In draft</h3>
              <p>
                Term describing the release status of a release once it’s been
                initially created and is being worked on by a Primary Analyst
                and other Production team members.
              </p>
              <p>
                See also <a href="#release-status">Release status</a>.
              </p>
              <h3 id="in-pre-release">In pre-release</h3>
              <p>
                Term describing the release status of a release once it’s been
                approved for sign-off by a Responsible Statistician and enters
                the Pre-release process.
              </p>
              <p>
                See also <a href="#pre-release-process">Pre-release process</a>.
              </p>
              <p>
                See also <a href="#release-status">Release status</a>.
              </p>
              <h3 id="independent-schools-adjudicator">
                independent schools adjudicator
              </h3>
              <p>Lower case.</p>
              <h3 id="individual-education-plan">individual education plan</h3>
              <p>Lower case.</p>
              <h3 id="individual-schools-budget">individual schools budget</h3>
              <p>Lower case.</p>
              <h3 id="initial-teacher-training">initial teacher training</h3>
              <p>Lower case.</p>
              <h3 id="inset-day">inset day</h3>
              <p>Lower case.</p>
              <h3 id="internal-release-notes">Internal notes</h3>
              <p>
                Term referring to the functionality which adds internal notes to
                releases within the Admin system to explain how a release has
                been updated.
              </p>
              <p>
                These notes are then displayed to a release’s Production team
                members to update them about the status of the release.
              </p>
              <p>
                See also <a href="#release-notes">Release notes</a>.
              </p>
              <h3 id="international-baccalaureate">
                International Baccalaureate
              </h3>
              <p>Upper case.</p>
              <h3 id="italics">Italics</h3>
              <p>
                Do not use italics. Use ‘single quotation marks’ if referring to
                a document, scheme or initiative.
              </p>
            </AccordionSection>
            <AccordionSection heading="J">
              <h3 id="job-titles">Job titles</h3>
              <p>
                Specific job titles and ministers’ role titles are upper case:
                Minister for Housing, Home Secretary.
              </p>
              <p>
                Generic job titles and ministers’ role titles are lower case:
                director, minister.
              </p>
              <p>
                See also <a href="#shadow-job-titles">Shadow job titles</a>
              </p>
            </AccordionSection>
            <AccordionSection heading="K">
              <div>
                <h3 id="kanban">kanban</h3>
                <p>
                  Upper case when referring to The Kanban Method, otherwise
                  lower case.
                </p>
                <h3 id="key-stage">key stage</h3>
                <p>Lower case and numeral: key stage 4.</p>
              </div>
            </AccordionSection>
            <AccordionSection heading="L">
              <h3 id="law">law</h3>
              <p>Lower case even when it’s ‘the law’.</p>
              <h3 id="legal-aid">legal aid</h3>
              <p>Lower case.</p>
              <h3 id="legal-content">Legal content</h3>
              <p>
                Legal content can still be written in plain English. It’s
                important that users understand content and that we present
                complicated information simply.
              </p>
              <p>
                If you have to publish legal jargon, it will be a publication so
                write a plain English summary.
              </p>
              <p>
                Where evidence shows there’s a clear user need for including a
                legal term (like bona vacantia), always explain it in plain
                English.
              </p>
              <p>
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#legal-content">
                  Read more about writing legal content
                </a>
              </p>
              <h3 id="legislative-competence-order">
                legislative competence order
              </h3>
              <p>
                Upper case if used as the full title: the National Assembly for
                Wales (Legislative Competence) (Social Welfare) Order 2008.
              </p>
              <p>
                Lower case otherwise: the legislative competence orders (LCOs)
                are approved, rejected or withdrawn.
              </p>
              <h3 id="liaison-officers">liaison officers</h3>
              <p>Lower case.</p>
              <h3 id="life-cycle">life cycle</h3>
              <p>Not “lifecycle” or “life-cycle”.</p>
              <h3 id="links">Links</h3>
              <p>
                Front-load your link text with the relevant terms and make them
                active and specific. Always link to online services first. Offer
                offline alternatives afterwards, when possible.
              </p>
              <h3 id="lists">Lists</h3>
              <p>
                Lists should be bulleted to make them easier to read.{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#bullet-points-steps">
                  See bullets and steps
                </a>
                .
              </p>
              <p>
                Very long lists can be written as a paragraph with a lead-in
                sentence if it looks better: ‘The following countries are in the
                EU: Spain, France, Italy…’
              </p>
              <h3 id="live">Live</h3>
              <p>
                Term describing the release status of a release once it’s gone
                through the pre-release process, been published and is ‘live’ on
                Explore education statistics.
              </p>
              <p>
                See also <a href="#release-status">Release status</a>.
              </p>
              <h3 id="local-authority">local authority</h3>
              <p>Lower case. Do not use LA.</p>
              <p>
                Use local council instead of local authority where possible.
              </p>
              <h3 id="local-authority-trading-standards-services">
                Local Authority Trading Standards Services
              </h3>
              <p>
                Upper case as long as it’s a specific named organisation, not
                trading standards services in general.
              </p>
              <h3 id="local-council">local council</h3>
              <p>Lower case.</p>
              <p>
                Use local council, instead of local authority where possible.
              </p>
              <h3 id="log-book">log book</h3>
              <p>Two words.</p>
              <h3 id="log-in">log in</h3>
              <p>
                See <a href="#sign-in-or-log-in">sign in or log in</a>.
              </p>
              <h3 id="looked-after-children">looked-after children</h3>
              <p>Hyphenated.</p>
            </AccordionSection>
            <AccordionSection heading="M">
              <h3 id="mainstream-schools">mainstream schools</h3>
              <p>Lower case.</p>
              <h3 id="maintained-schools-maintained-nursery-schools">
                maintained schools, maintained nursery schools
              </h3>
              <p>Lower case.</p>
              <h3 id="mark-scheme-mark-sheet">mark scheme, mark sheet</h3>
              <p>Lower case.</p>
              <h3 id="maths-content">Maths content</h3>
              <p>Use a minus sign for negative numbers: –6</p>
              <p>Ratios have no space either side of the colon: 5:12</p>
              <p>
                One space each side of symbols: +, –, ×, ÷ and = (so: 2 + 2 = 4)
              </p>
              <p>
                Use the minus sign for subtraction. Use the correct symbol for
                the multiplication sign (×), not the letter x.
              </p>
              <p>
                Write out and hyphenate fractions: two-thirds, three-quarters.
              </p>
              <p>
                Write out decimal fractions as numerals. Use the same number
                format for a sequence: 0.75 and 0.45
              </p>
              <h3 id="measurements">Measurements</h3>
              <p>Use numerals and spell out measurements at first mention.</p>
              <p>
                Do not use a space between the numeral and abbreviated
                measurement: 3,500kg not 3,500 kg.
              </p>
              <p>
                <a href="#">Contact the BAU team</a> if you need to follow
                different conventions, for example you’re writing just for
                scientists or engineers.
              </p>
              <p>
                Abbreviating kilograms to kg is fine - you do not need to spell
                it out.
              </p>
              <p>
                If the measurement is more than one word, like kilometres per
                hour, then spell it out the first time it’s used with the
                abbreviation. From then on, abbreviate. If it’s only mentioned
                once, do not abbreviate.
              </p>
              <h3 id="metadata">metadata</h3>
              <p>Not “meta data”.</p>
              <h3 id="metaphors">metaphors</h3>
              <p>
                See <a href="#words-to-avoid">words to avoid</a>
              </p>
              <h3 id="millions">Millions</h3>
              <p>Always use million in money (and billion): £138 million.</p>
              <p>Use millions in phrases: millions of people.</p>
              <p>
                But do not use £0.xx million for amounts less than £1 million.
              </p>
              <p>Do not abbreviate million to m.</p>
              <h3 id="minimum-viable-product-mvp">
                minimum viable product (MVP)
              </h3>
              <p>
                Term describing the end-product of the beta stage of the DfE
                Data Dissemination Project.{' '}
              </p>
              <p>
                Essentially, an online version of Explore education statistics
                with enough features to satisfy early customers and to provide
                feedback for its future development as a service.
              </p>
              <h3 id="minister">minister</h3>
              <p>
                Use upper case for the full title, like Minister for Overseas
                Development, or when used with a name, as a title, like Health
                Minister Norman Lamb.
              </p>
              <p>
                When used without the name, shortened titles are lower case: The
                health minister welcomed the research team.
              </p>
              <h3 id="mixed-age-class">mixed-age class</h3>
              <p>Hyphenated.</p>
              <h3 id="mixed-sex-schools">mixed-sex schools</h3>
              <p>Hyphenated.</p>
              <h3 id="modern-foreign-languages">modern foreign languages</h3>
              <p>Lower case.</p>
              <h3 id="money">money</h3>
              <p>Use the £ symbol: £75</p>
              <p>
                Do not use decimals unless pence are included: £75.50 but not
                £75.00
              </p>
              <p>
                Do not use ‘£0.xx million’ for amounts less than £1 million.
              </p>
              <p>
                Write out pence in full: calls will cost 4 pence per minute from
                a landline.
              </p>
              <p>Currencies are lower case.</p>
              <h3 id="months">Months</h3>
              <p>
                See <a href="#dates">Dates</a>.
              </p>
              <h3 id="mp">MP</h3>
              <p>Do not use Member of Parliament, just MP.</p>
              <h3 id="multi-academy-trust">multi-academy trust</h3>
              <p>Hyphenated.</p>
              <h3 id="multi-disciplinary">multi-disciplinary</h3>
              <p>Hyphenated.</p>
              <h3 id="multi-ethnic">multi-ethnic</h3>
              <p>Hyphenated.</p>
              <h3 id="multi-year-funding">multi-year funding</h3>
              <p>Hyphenated.</p>
              <h3 id="multilingual">multilingual</h3>
              <p>One word.</p>
              <h3 id="mvp">MVP</h3>
              <p>
                See{' '}
                <a href="#minimum-viable-product-mvp">
                  minimum viable product (MVP)
                </a>
                .
              </p>
            </AccordionSection>
            <AccordionSection heading="N">
              <h3 id="na">N/A</h3>
              <p>Separate with a slash. Only use in tables.</p>
              <h3 id="national-curriculum">national curriculum</h3>
              <p>Lower case.</p>
              <h3 id="national-curriculum-tests">national curriculum tests</h3>
              <p>Do not call them SATs.</p>
              <h3 id="national-living-wage">National Living Wage</h3>
              <p>Upper case.</p>
              <h3 id="national-minimum-wage">National Minimum Wage</h3>
              <p>Upper case.</p>
              <h3 id="national-occupational-standards">
                national occupational standards
              </h3>
              <p>Lower case.</p>
              <h3 id="national-pupil-database">national pupil database</h3>
              <p>Lower case.</p>
              <h3 id="national-scholarship-fund">national scholarship fund</h3>
              <p>Lower case.</p>
              <h3 id="newly-qualified-teacher">newly qualified teacher</h3>
              <p>Lower case.</p>
              <h3 id="the-north-the-north-of-england">
                the north, the north of England
              </h3>
              <p>Lower case.</p>
              <h3 id="north-east-north-west">north-east, north-west</h3>
              <p>Lower case, hyphenated.</p>
              <h3 id="north-wales">north Wales</h3>
              <p>Not a specific region of the UK.</p>
              <h3 id="numbers">Numbers</h3>
              <p>
                Use ‘one’ unless you’re talking about a step, a point in a list
                or another situation where using the numeral makes more sense:
                ‘in point 1 of the design instructions’, for example. Or this:
              </p>
              <p>You’ll be shown 14 clips that feature everyday road scenes.</p>
              <p>There will be:</p>
              <ul>
                <li>1 developing hazard in 13 clips</li>
                <li>2 developing hazards in the other clip</li>
              </ul>
              <p>
                Write all other numbers in numerals (including 2 to 9) except
                where it’s part of a common expression like ‘one or two of them’
                where numerals would look strange.
              </p>
              <p>
                If a number starts a sentence, write it out in full
                (Thirty-four, for example) except where it starts a title or
                subheading.
              </p>
              <p>For numerals over 999 - insert a comma for clarity: 9,000</p>
              <p>Spell out common fractions like one-half.</p>
              <p>Use a % sign for percentages: 50%</p>
              <p>Use a 0 where there’s no digit before the decimal point.</p>
              <p>Use ‘500 to 900’ and not ‘500-900’ (except in tables).</p>
              <p>Use MB for anything over 1MB: 4MB not 4096KB.</p>
              <p>Use KB for anything under 1MB: 569KB not 0.55MB.</p>
              <p>
                Keep it as accurate as possible and up to 2 decimal places:
                4.03MB.
              </p>
              <p>
                Addresses: use ‘to’ in address ranges: 49 to 53 Cherry Street.
              </p>
              <h4 id="ordinal-numbers">Ordinal numbers</h4>
              <p>
                Spell out first to ninth. After that use 10th, 11th and so on.
              </p>
              <p>In tables, use numerals throughout.</p>
              <h3 id="nursery-school">nursery school</h3>
              <p>Lower case.</p>
            </AccordionSection>
            <AccordionSection heading="O">
              <h3 id="ofsted-judgements">Ofsted judgements</h3>
              <p>
                Lower case and not in inverted commas: Westminster School was
                judged outstanding in its latest Ofsted inspection.
              </p>
              <p>There are 4 Ofsted grades:</p>
              <ul>
                <li>outstanding (or grade 1)</li>
                <li>good (or grade 2)</li>
                <li>requires improvement (or grade 3)</li>
                <li>inadequate (or grade 4)</li>
              </ul>
              <h3 id="one-year-on">one-year-on</h3>
              <p>If used adjectivally, hyphenate and use one rather than 1.</p>
              <h3 id="online">online</h3>
              <p>One word.</p>
              <h3 id="or">or</h3>
              <p>
                Do not use slashes instead of “or”. For example, “Do this 3/4
                times”.
              </p>
              <h3 id="organisations">Organisations</h3>
              <p>
                All organisations are singular: The government has decided to
                sell assets.
              </p>
              <p>
                ‘They’ should be used as a pronoun: ‘HMPO is the sole issuer of
                UK passports. They will send your new passport within 3 weeks.’
              </p>
              <p>
                The definite article can be used when referring to the
                organisation by its full name, but should not be used with the
                organisation’s acronym: ‘You should contact the Driver and
                Vehicle Standards Agency if…’ but ‘You should contact DVSA if…’
              </p>
              <p>
                Use local council, instead of local authority, where possible.
              </p>
              <h3 id="other-production-team-member">
                Other production team member
              </h3>
              <p>
                Role of person within a ‘Production team’ who may be responsible
                for reviewing or collaborating on creating section of a release
                for Explore education statistics.
              </p>
              <p>
                See also <a href="#production-team">Production team</a>.
              </p>
              <h3 id="overseas-trained-teacher">overseas-trained teacher</h3>
              <p>Lower case. Hyphenated.</p>
            </AccordionSection>
            <AccordionSection heading="P">
              <h3 id="paper-b">Paper B</h3>
              <p>In national curriculum tests.</p>
              <h3 id="parliament">Parliament</h3>
              <p>Upper case.</p>
              <h3 id="parliamentary-committees">Parliamentary committees</h3>
              <p>
                Parliamentary is upper case and committees is in lower case.
              </p>
              <h3 id="pathfinder">pathfinder</h3>
              <p>Lower case.</p>
              <h3 id="pdf">PDF</h3>
              <p>Upper case. No need to explain the acronym.</p>
              <h3 id="penalty">penalty</h3>
              <p>
                See the entry for{' '}
                <a href="/guidance/style-guide/a-to-z-of-gov-uk-style#fine">
                  ‘fine’
                </a>
                .
              </p>
              <h3 id="per-cent">Per cent</h3>
              <p>
                Use per cent not percent. Percentage is one word. Always use %
                with a number.
              </p>
              <h3 id="performance-management">performance management</h3>
              <p>Lower case.</p>
              <h3 id="performance-tables">performance tables</h3>
              <p>Lower case.</p>
              <h3 id="physical-education-or-pe">physical education or PE</h3>
              <p>You can write in full or use the initials.</p>
              <h3 id="plain-english">plain English</h3>
              <p>
                Lower case plain and upper case English unless in a title: the
                Plain English Campaign.
              </p>
              <p>
                All content on explore ediucation statistics should be written
                in{' '}
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#plain-english">
                  plain English
                </a>
                . You should also make sure you use language your audience will
                understand - check which{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#words-to-avoid">
                  words you should avoid
                </a>
                .
              </p>
              <h3 id="policy-note">policy note</h3>
              <p>Lower case.</p>
              <h3 id="policy-statement">policy statement</h3>
              <p>Lower case.</p>
              <h3 id="powerpoint-presentation">PowerPoint presentation</h3>
              <p>Upper case because PowerPoint is a brand name.</p>
              <h3 id="pre-school">pre-school</h3>
              <p>Hyphenated.</p>
              <h3 id="primary-analyst">Primary Analyst</h3>
              <p>
                Role of person (ie main primary analyst / statistician) within a
                ‘Production team’ who’s responsible for setting up and creating
                a release on Explore education statistics.
              </p>
              <p>
                See also <a href="#production-team">Production team</a>.
              </p>
              <h3 id="prime-minister">Prime Minister</h3>
              <p>Use Prime Minister Boris Johnson and the Prime Minister.</p>
              <h3 id="production-team">Production team</h3>
              <p>
                Group of people with roles who are responsible for creating and
                publishing a release on Explore education statistics.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <p>Made up of the following roles: </p>
                <li>Other production team member</li>
                <li>Primary Analyst</li>
                <li>Responsible Statistician</li>
                <li>Team Leader</li>
              </ul>
              <p>
                See also{' '}
                <a href="#other-production-team-membe">
                  Other production team member
                </a>
                .
              </p>
              <p>
                See also <a href="#primary-analyst">Primary Analyst</a>.
              </p>
              <p>
                See also{' '}
                <a href="#responsible-statistician">Responsible Statistician</a>
                .
              </p>
              <p>
                See also <a href="#team-leader">Team Leader</a>.
              </p>
              <h3 id="programme">programme</h3>
              <p>
                Lower case: Troubled Families programme, Sure Start programme.
              </p>
              <h3 id="progress-8-measure">Progress 8 measure</h3>
              <p>Upper case P, lower case m.</p>
              <h3 id="public-health">public health</h3>
              <p>Lower case.</p>
              <h3 id="public-sector">public sector</h3>
              <p>Lower case.</p>
              <h3 id="publications">Publications</h3>
              <p>
                Internal name for the high-level category of statistics and data
                which contain releases within the admin system.
              </p>
              <p>Only a Team Leader can create a new publication.</p>
              <p>
                Should only be used as a term internally as research has shown
                users have no understanding of what a ‘publication’ refers to.
              </p>
              <p>
                Therefore, should not be used within published content on the
                service.
              </p>
              <h3 id="pupil-premium">pupil premium</h3>
              <p>Lower case.</p>
              <h3 id="pupil-referral-unit">pupil referral unit</h3>
              <p>Lower case.</p>
            </AccordionSection>
            <AccordionSection heading="Q">
              <h3 id="qualified-teacher-status">qualified teacher status</h3>
              <p>Lower case.</p>
              <h3 id="quotes-speech-marks">Quotes and speech marks</h3>
              <p>
                In long passages of speech, open quotes for every new paragraph,
                but close quotes only at the end of the final paragraph.
              </p>
              <h4 id="single-quotes">Single quotes</h4>
              <p>Use single quotes:</p>
              <ul>
                <li>in headlines</li>
                <li>for unusual terms</li>
                <li>
                  when referring to words or publications, for example:
                  ‘Download the publication ‘Understanding Capital Gains Tax’
                  (PDF, 360KB)’
                </li>
              </ul>
              <h4 id="double-quotes">Double quotes</h4>
              <p>Use double quotes in body text for direct quotations.</p>
              <h4 id="block-quotes">Block quotes</h4>
              <p>
                Use the{' '}
                <a href="https://www.gov.uk/guidance/how-to-publish-on-gov-uk/markdown#blockquotes">
                  block quote Markdown
                </a>{' '}
                for quotes longer than a few sentences.
              </p>
            </AccordionSection>
            <AccordionSection heading="R">
              <h3 id="ready-for-sign-off">Ready for sign-off</h3>
              <p>
                Term describing the release status of a release once it’s been
                reviewed by the members of a Production team and is ready for
                sign-off by a Responsible Statistician.
              </p>
              <p>
                See also <a href="#release-status">Release status</a>
              </p>
              <p>
                See also{' '}
                <a href="#responsible-statistician">Responsible Statistician</a>
              </p>
              <h3 id="release">release</h3>
              <p>
                Internal name for the service end product, created by a
                ‘Production team’ to publish the latest ‘release’ of statistics
                and data on the service.
              </p>
              <p>
                Should only be used as a term internally as research has shown
                users have no understanding of what a ‘release’ refers to.
              </p>
              <p>
                Therefore, should not be used as a noun within published content
                on the service.
              </p>
              <p>
                Instead, a ‘release’ should only be used as a verb to describe
                the latest ‘release’ of statistics and data to be published on
                the service.
              </p>
              <h3 id="release-footnotes">Release footnotes</h3>
              <p>
                Term referring to the functionality which adds specific
                user-friendly footnotes to tables and charts within the Admin
                system and then displays them for users within releases on
                Explore education statistics.
              </p>
              <p>
                See also <a href="#footnotes">Footnotes</a>.
              </p>
              <h3 id="release-notes">Release notes</h3>
              <p>
                Term referring to the functionality which adds user-friendly
                notes to releases within the Admin system to explain how a
                release’s content or data has been updated.
              </p>
              <p>
                These notes are then time-stamped and displayed to users under
                the ‘About these statistics’ section of a published release on
                Explore education statistics for transparency purposes.
              </p>
              <p>
                See also <a href="#internal-release-notes">Internal notes</a>.
              </p>
              <h3 id="release-status">Release status</h3>
              <p>
                Catch-all term describing the status of a release within the
                publishing process including:{' '}
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>In draft</li>
                <li>Ready for sign-off</li>
                <li>Approved for release</li>
                <li>In pre-release</li>
                <li>Live</li>
              </ul>
              <h3 id="reproducible-analytical-pipelines-rap">
                Reproducible Analytical Pipelines (RAP)
              </h3>
              <p>
                An automated statistical release which can be easily reproduced,
                tested, and audited without using a manual or semi-manual
                processes.
              </p>
              <h3 id="responsible-statistician">Responsible Statistician</h3>
              <p>
                Role of person (ie Grade 6 or above) within a ‘Production team’
                responsible for signing-off a release once it’s been passed for
                higher review by a Primary Analyst or Team Leader.
              </p>
              <p>
                see also <a href="#primary-analyst">higher review</a>.
              </p>
              <p>
                see also <a href="#primary-analyst">Primary Analyst</a>.
              </p>
              <p>
                see also <a href="#production-team">Production team</a>.
              </p>
            </AccordionSection>
            <AccordionSection heading="S">
              <h3 id="same-sex-schools">same-sex schools</h3>
              <p>Hyphenated.</p>
              <h3 id="sat-nav">sat nav</h3>
              <p>Two words, lower case.</p>
              <h3 id="sats">SATs</h3>
              <p>
                <a href="#national-curriculum-tests">
                  See national curriculum tests
                </a>
                .
              </p>
              <h3 id="school-admissions-code">School Admissions Code</h3>
              <p>
                Upper case. After the first mention you can refer to it in lower
                case: the admissions code or the code.
              </p>
              <h3 id="school-and-college-performance-tables">
                school and college performance tables
              </h3>
              <p>Lower case.</p>
              <h3 id="school-improvement-plan">school improvement plan</h3>
              <p>Lower case.</p>
              <h3 id="school-subjects">school subjects</h3>
              <p>Lower case for all except languages and initialisations.</p>
              <h3 id="schools-workforce">schools workforce</h3>
              <p>No apostrophe as it’s an attributive noun.</p>
              <h3 id="schoolwork">schoolwork</h3>
              <p>One word.</p>
              <h3 id="science-and-technical-advice-cell">
                science and technical advice cell
              </h3>
              <p>Lower case.</p>
              <h3 id="scientific-names">Scientific names</h3>
              <p>
                Capitalise the first letter of the first part of the scientific
                name. Do not use italics.
              </p>
              <h3 id="seasons">seasons</h3>
              <p>spring, summer, autumn, winter are lower case.</p>
              <h3 id="secretary-of-state-for-xxx">
                Secretary of State for XXX
              </h3>
              <p>
                The Secretary of State for XXX is upper case whether or not it’s
                used with the holder’s name because there is only one. Use
                common sense to capitalise shortened versions of the SoS titles
                such as Health Secretary. The rule for ministers is different
                because there is more than one.
              </p>
              <h3 id="section-2">section 2</h3>
              <p>As in part of an act or a strategy.</p>
              <h3 id="self-assessment">self-assessment</h3>
              <p>
                This compound noun should be hyphenated, unless it’s an HMRC
                title.
              </p>
              <h3 id="semicolons">semicolons</h3>
              <p>
                Do not use semicolons as they are often mis-read. Long sentences
                using semicolons should be broken up into separate sentences
                instead.
              </p>
              <h3 id="sentence-length">Sentence length</h3>
              <p>
                Do not use long sentences. Check sentences with more than 25
                words to see if you can split them to make them clearer.
              </p>
              <p>
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#short-sentences">
                  Read more about short sentences
                </a>
                .
              </p>
              <h3 id="sign-in-or-log-in">sign in or log in</h3>
              <p>
                Use sign in rather than log in (verb) for calls-to-action where
                users enter their details to access a service.
              </p>
              <p>
                Do not use login as a noun - say what the user actually needs to
                enter (like username, password, National Insurance number).
              </p>
              <h3 id="to-19-bursary-fund">16 to 19 Bursary Fund</h3>
              <p>
                Upper case. After the first mention you can refer to it in lower
                case: the fund.
              </p>
              <h3 id="sixth-former">sixth former</h3>
              <p>Not hyphenated.</p>
              <h3 id="sixth-form-college">sixth-form college</h3>
              <p>Hyphenated. Lower case.</p>
              <h3 id="smes">SMEs</h3>
              <p>
                This acronym means small and medium-sized enterprises. Use SME
                for the singular.
              </p>
              <h3 id="south-the-south-of-england">
                south, the south of England
              </h3>
              <p>Lower case.</p>
              <h3 id="south-east-south-west">south-east, south-west</h3>
              <p>Lower case, hyphenated.</p>
              <h3 id="spaces">spaces</h3>
              <p>One space after a full stop, not 2.</p>
              <h3 id="special-educational-needsspecial-educational-needs-and-disabilities-send">
                special educational needs/special educational needs and
                disabilities (SEN/D)
              </h3>
              <p>Lower case, but use upper case for the acronym.</p>
              <h3 id="special-educational-needs-code-of-practice">
                Special Educational Needs Code of Practice
              </h3>
              <p>
                Upper case. When not using the full title in subsequent
                mentions, refer to it in lower case: the code of practice or the
                code.
              </p>
              <h3 id="special-measures">special measures</h3>
              <p>Lower case.</p>
              <h3 id="speech-marks">Speech marks</h3>
              <p>
                See <a href="#quotes-speech-marks">‘Quotes and speech marks’</a>
              </p>
              <h3 id="spending-review">Spending Review</h3>
              <p>
                Upper case for the 5-year view of the government’s spending
                plans. Lower case in other contexts: we are conducting a
                spending review.
              </p>
              <h3 id="standards-of-conduct">standards of conduct</h3>
              <p>Lower case.</p>
              <h3 id="start-page">start page</h3>
              <p>
                Official GDS web page design pattern used as starting point for
                any digital service including Explore education statistics.
              </p>
              <p>
                See{' '}
                <a href="https://design-system.service.gov.uk/patterns/start-pages/ ">
                  GOV.UK Design System: Start pages
                </a>
                .
              </p>
              <h3 id="statement-of-send">statement of SEND</h3>
              <p>Lower case.</p>
              <h3 id="statistical-first-release">statistical first release</h3>
              <p>Lower case.</p>
              <h3 id="statistics">Statistics</h3>
              <p>
                Read{' '}
                <a rel="external" href="http://style.ons.gov.uk">
                  Style.ONS
                </a>{' '}
                to find out how to write about statistics. This has been
                produced by the Office for National Statistics for all members
                of the Government Statistical Service.
              </p>
              <p>
                Term describing the content published on explore education
                statistics which interprets and provides a commentary on and
                summary of its related data.
              </p>
              <p>
                External name for the service end product, created by a
                ‘Production team’ when it publishes the latest ‘release’ of
                ‘statistics and data’ on the service.
              </p>
              <p>
                Should be used externally as a noun to refer to what is
                published for users on Explore education statistics.
              </p>
              <p>
                Research has shown users understand what ‘statistics’ refers to
                in comparison to the former interchangeable use or ‘publication’
                and ‘release’ for the same end product.
              </p>
              <p>
                Upper case National Statistics for the official statistics
                quality mark. Lower case for anything else, including statistics
                that are national in scope.
              </p>
              <h3 id="statistics-modernisation-team">
                Statistics Modernisation Team
              </h3>
              <p>
                Official internal name of the team behind the project building
                Explore education statistics.
              </p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="steps-1">steps</h3>
              <p>
                See <a href="#bullet-points-steps">Bullet points and steps</a>
              </p>
              <h3 id="strategic-national-framework-on-xxx">
                strategic national framework on XXX
              </h3>
              <p>Lower case.</p>
              <h3 id="strategic-partners">strategic partners</h3>
              <p>Not a title.</p>
              <h3 id="strategy">strategy</h3>
              <p>
                Lower case. Do not capitalise a named strategy: national health
                and welfare strategy.
              </p>
              <h3 id="studio-school">studio school</h3>
              <p>Lower case.</p>
              <h3 id="study-programme">study programme</h3>
              <p>Lower case.</p>
              <h3 id="summaries">Summaries</h3>
              <p>Summaries should:</p>
              <ul>
                <li>be 140 characters or less</li>
                <li>end with a full stop</li>
                <li>not repeat the title or body text</li>
                <li>be clear and specific</li>
              </ul>
              <h3 id="summary-of-consultation-responses">
                summary of consultation responses
              </h3>
              <p>All lower case.</p>
              <h3 id="summer-school">summer school</h3>
              <p>Lower case.</p>
              <h3 id="sure-start-programme">Sure Start programme</h3>
              <p>
                Upper case because it’s the name of a programme, but programme
                is lower case.
              </p>
            </AccordionSection>
            <AccordionSection heading="T">
              <h3 id="t-level">T level</h3>
              <p>No hyphen. Lower case level.</p>
              <h3 id="tables">Tables</h3>
              <p>
                Term referring to the table functionality available within the
                Admin system and on Explore education statistics.
              </p>
              <h3 id="tax-credits">tax credits</h3>
              <p>
                Lower case and plural. Working Tax Credit and Child Tax Credit
                are specific benefits, so are upper case and singular.
              </p>
              <h3 id="the-teachers-standards">the teachers’ standards</h3>
              <p>Lower case.</p>
              <h3 id="teaching-school">teaching school</h3>
              <p>Lower case.</p>
              <h3 id="team">team</h3>
              <p>
                Lower case: youth offending team, Behavioural Insights team.
              </p>
              <h3 id="team-leader">Team leader</h3>
              <p>
                Role of person (ie team leader of main primary analyst /
                statistician) within a ‘Production team’ responsible for
                creating new publications and with a focus on clearing releases
                so they’re ready for higher review.
              </p>
              <p>
                See also <a href="#primary-analyst">higher review</a>.
              </p>
              <p>
                See also <a href="#production-team">Production team</a>.
              </p>
              <h3 id="teamwork">teamwork</h3>
              <p>Lower case. One word.</p>
              <h3 id="tech-levels">tech levels</h3>
              <p>
                Lower case. The name given to the occupational qualifications
                endorsed by employers and trade associations.
              </p>
              <h3 id="technical-level-qualifications">
                technical level qualifications
              </h3>
              <p>Lower case.</p>
              <h3 id="techbacc">TechBacc</h3>
              <p>A performance measure of level 3 vocational qualifications.</p>
              <h3 id="technical-terms">technical terms</h3>
              <p>
                Use technical terms where you need to. They’re not jargon. You
                just need to explain what they mean the first time you use them.
              </p>
              <p>
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#writing-specialists">
                  Read more about writing for specialists
                </a>
                .
              </p>
              <h3 id="telephone-numbers">Telephone numbers</h3>
              <p>Use Telephone: 011 111 111 or Mobile: - not Mob:.</p>
              <p>
                Use spaces between city and local exchange. Here are the
                different formats to use:
              </p>
              <p>01273 800 900</p>
              <p>020 7450 4000</p>
              <p>0800 890 567</p>
              <p>07771 900 900</p>
              <p>077718 300 300</p>
              <p>+44 (0)20 7450 4000</p>
              <p>+39 1 33 45 70 90</p>
              <p>
                When a number is memorable, group the numbers into easily
                remembered units: 0800 80 70 60.
              </p>
              <h3 id="the-explore-education-statistics-service">
                The Explore education statistics service
              </h3>
              <p>
                Alternative and full generic internal name for explore education
                statistics.
              </p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="themes">Themes</h3>
              <p>
                Categorical term for the high-level primary collection of
                publications within the admin system. The parent of its related
                admin system ‘topics’.
              </p>
              <p>
                For example, ‘Pupils and schools’ is the parent theme of the
                ‘Admission appeals’, ‘Exclusions’ and ‘Pupil absence’ topics.
              </p>
              <p>
                See also<a href="#topics">topics</a>.
              </p>
              <h3 id="the-service">the service</h3>
              <p>
                Alternative generic internal name for explore education
                statistics.
              </p>
              <p>
                See{' '}
                <a href="#explore-education-statistics">
                  Explore education statistics
                </a>
                .
              </p>
              <h3 id="threshold-assessment">threshold assessment</h3>
              <p>Lower case.</p>
              <h3 id="times">Times</h3>
              <ul>
                <li>
                  use ‘to’ in time ranges, not hyphens, en rules or em dashes:
                  10am to 11am (not 10-11am)
                </li>
                <li>5:30pm (not 1730hrs)</li>
                <li>midnight (not 00:00)</li>
                <li>midday (not 12 noon, noon or 12pm)</li>
                <li>6 hours 30 minutes</li>
              </ul>
              <p>
                Midnight is the first minute of the day, not the last. You
                should consider using “11:59pm” to avoid confusion about a
                single, specific time.
              </p>
              <p>
                For example, “You must register by 11:59pm on Tuesday 14 June.”
                can only be read one way, but “You must register by midnight on
                Tuesday 14 June” can be read in two ways (the end of Monday 13,
                or end of Tuesday 14).
              </p>
              <h3 id="titles">Titles</h3>
              <p>Page titles should:</p>
              <ul>
                <li>be 65 characters or less</li>
                <li>be unique, clear and descriptive</li>
                <li>be front-loaded and optimised for search</li>
                <li>use a colon to break up longer titles</li>
                <li>not contain dashes or slashes</li>
                <li>not have a full stop at the end</li>
                <li>not use acronyms unless they are well-known, like EU</li>
              </ul>
              <h3 id="topics">Topics</h3>
              <p>
                Categorical term for the high-level secondary collection of
                publications within the admin system. The child of a related
                admin system ‘theme’.
              </p>
              <p>
                For example, ‘Admission appeals’, ‘Exclusions’ and ‘Pupil
                absence’ are the child topics of the ‘Pupils and schools’ theme.
              </p>
              <p>
                See also <a href="#themes">themes</a>.
              </p>
              <h3 id="town-council">town council</h3>
              <p>Lower case, even when part of a name: Swanage town council.</p>
              <h3 id="training-schools">training schools</h3>
              <p>Lower case.</p>
              <h3 id="travellers">Travellers</h3>
              <p>
                Upper case because Irish Travellers are recognised as an ethnic
                group under the Race Relations Act. New age travellers is lower
                case.
              </p>
              <h3 id="trust-school">trust school</h3>
              <p>Lower case.</p>
            </AccordionSection>
            <AccordionSection heading="U">
              <h3 id="uk-government">UK government</h3>
              <p>Never HM government.</p>
              <h3 id="underachiever">underachiever</h3>
              <p>One word.</p>
              <h3 id="underlying-data">Underlying Data</h3>
              <p>Alternative internal name for data.</p>
              <p>
                See <a href="#data">data</a>.
              </p>
              <h3 id="underperforming">underperforming</h3>
              <p>One word.</p>
              <h3 id="unique-pupil-number">unique pupil number</h3>
              <p>Lower case.</p>
              <h3 id="university-technical-college">
                university technical college
              </h3>
              <p>Lower case.</p>
              <h3 id="users">users</h3>
              <p>General name for anyone who uses the service.</p>
            </AccordionSection>
            <AccordionSection heading="V">
              <h3 id="voluntary-aided-schools-voluntary-controlled-schools">
                voluntary-aided schools, voluntary-controlled schools
              </h3>
              <p>Hyphenated. Lower case.</p>
            </AccordionSection>
            <AccordionSection heading="W">
              <h3 id="the-west-western-europe">the west, western Europe</h3>
              <p>Lower case.</p>
              <h3 id="west-end-london">West End (London)</h3>
              <p>Upper case.</p>
              <h3 id="white-paper">white paper</h3>
              <p>Lower case.</p>
              <h3 id="words-to-avoid">Words to avoid</h3>
              <p>
                Plain English is mandatory for all of GOV.UK so please avoid
                using these words:
              </p>
              <ul>
                <li>agenda (unless it’s for a meeting)</li>
                <li>advancing</li>
                <li>collaborate (use working with)</li>
                <li>combating</li>
                <li>
                  commit/pledge (we need to be more specific - we’re either
                  doing something or we’re not)
                </li>
                <li>countering</li>
                <li>
                  deliver (pizzas, post and services are delivered - not
                  abstract concepts like improvements or priorities)
                </li>
                <li>deploy (unless it’s military or software)</li>
                <li>dialogue (we speak to people)</li>
                <li>disincentivise (and incentivise)</li>
                <li>empower</li>
                <li>
                  facilitate (instead, say something specific about how you’re
                  helping)
                </li>
                <li>focusing</li>
                <li>foster (unless it’s children)</li>
                <li>
                  impact (do not use this as a synonym for have an effect on, or
                  influence)
                </li>
                <li>initiate</li>
                <li>
                  key (unless it unlocks something. A subject/thing is not key -
                  it’s probably important)
                </li>
                <li>
                  land (as a verb only use if you’re talking about aircraft)
                </li>
                <li>leverage (unless in the financial sense)</li>
                <li>liaise</li>
                <li>overarching</li>
                <li>progress (as a verb - what are you actually doing?)</li>
                <li>
                  promote (unless you’re talking about an ad campaign or some
                  other marketing promotion)
                </li>
                <li>robust</li>
                <li>slimming down (processes do not diet)</li>
                <li>streamline</li>
                <li>
                  strengthening (unless it’s strengthening bridges or other
                  structures)
                </li>
                <li>
                  tackling (unless it’s rugby, football or some other sport)
                </li>
                <li>
                  transforming (what are you actually doing to change it?)
                </li>
                <li>utilise</li>
              </ul>
              <p>
                Avoid using metaphors – they do not say what you actually mean
                and lead to slower comprehension of your content. For example:
              </p>
              <ul>
                <li>
                  drive (you can only drive vehicles, not schemes or people)
                </li>
                <li>drive out (unless it’s cattle)</li>
                <li>
                  going forward (it’s unlikely we are giving travel directions)
                </li>
                <li>in order to (superfluous - do not use it)</li>
                <li>one-stop shop (we are government, not a retail outlet)</li>
                <li>ring fencing</li>
              </ul>
              <p>
                With all of these words you can generally replace them by
                breaking the term into what you’re actually doing. Be open and
                specific.
              </p>
              <p>
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#plain-english">
                  Read more about plain English and words to avoid
                </a>
                .
              </p>
              <h3 id="written-ministerial-statement-written-statement">
                written ministerial statement, written statement
              </h3>
              <p>Lower case.</p>
            </AccordionSection>
            <AccordionSection heading="Y">
              <h3 id="year-1-year-2">year 1, year 2</h3>
              <p>Lower case.</p>
            </AccordionSection>
          </Accordion>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationGlossary;
