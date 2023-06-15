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
        { name: 'Style guide' },
      ]}
      title="Style guide"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Style guidance</span>
            <h1 className="govuk-heading-xl">Style guide</h1>
          </div>
          <p>
            Browse our A to Z list of style, spelling and grammar conventions
            for all content published on the Explore education statistics
            service.
          </p>

          <Accordion id="a-z">
            <AccordionSection heading="A">
              <h3>&amp;</h3>
              <p>
                See <a href="#ampersand">Ampersand</a>
              </p>
              <h3 id="a42-a42s">A*, A*s</h3>
              <p>
                The top 123 grade in GCSEs and A levels. Use the symbol * not
                the word ‘star’. No apostrophe in the plural.
              </p>
              <h3 id="a-level">A level</h3>
              <p>No hyphen. Lower case level.</p>
              <h3 id="abbreviations-and-acronyms">
                Abbreviations and acronyms
              </h3>
              <p>
                Use abbreviations and acronyms for organisations and terms that
                are well-known and appear frequently.
              </p>
              <p>
                This includes government departments or schemes. For example,
                DfE, GCSEs, ONS, SEN, UK.
              </p>
              <p>
                For less well-known abbreviations and acronyms, write the name
                or term out in full the first time you use it, followed by the
                abbreviation in brackets.
              </p>
              <p>
                After that, use the abbreviation. Acronyms need to be written
                out in full again the first instance in each section of your
                article or page.
              </p>
              <p>
                For example, ‘The Labour Force Survey (LFS) is a continuous
                survey. Users of the LFS…’
              </p>
              <p>Do not use full stops or italics in abbreviations.</p>
              <p>
                If you think an acronym is well-known provide evidence that 80%
                of your users will understand and commonly use it. Evidence can
                be from search analytics or testing of a representative sample.
              </p>
              <p>
                As per{' '}
                <a href="#abbreviations-and-acronyms">
                  GDS style guide - Abbreviations and acronyms
                </a>{' '}
                and{' '}
                <a href="https://style.ons.gov.uk/category/house-style/language-and-spelling/#abbreviations">
                  ONS style guide - Abbreviations
                </a>
                .
              </p>
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
                Use the active rather than passive voice. This will help you
                write concise, clear content.
              </p>
              <p>
                Use active verbs and not passive verbs. Active verbs are when
                the sentence’s subject does something.
              </p>
              <p>
                Passive verbs are when the sentence’s subject has something done
                to it.
              </p>
              <p>For example:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  ‘The policy encourages schools to…’ and ‘The statistics show a
                  trend…’
                </li>
                <p className="govuk-!-font-size-19 govuk-!-font-weight-bold">
                  NOT
                </p>
                <li>
                  ‘Firms are encouraged by the policy to…’ and ‘A trend is shown
                  by the statistics…’
                </li>
              </ul>
              <p>
                As per{' '}
                <a href="https://style.ons.gov.uk/category/writing-for-the-web/tone-and-voice/">
                  ONS style guide - Tone and voice
                </a>
              </p>
              <h3 id="addressing-the-user">Addressing the user</h3>
              <p>
                Address the user as ‘you’ where possible as this makes your
                content more conversational and engaging.
              </p>
              <h3 id="ages">ages</h3>
              <p>
                Use the format ‘aged [age] years’. For example, aged nine years.
              </p>
              <p>
                Use the format ‘aged [age] to [age] years’. For example, aged 10
                to 11 years.
              </p>
              <p>
                Include the months or weeks for ages under a year. For example,
                aged nine weeks.
              </p>
              <p>
                If you refer to ages as “‘[age]-year-old’, include the hyphens.
                For example, 24-year-old or 16- to 24-year-old men.
              </p>
              <p>
                Write decades as an age as numerals. For example, women in their
                40s.
              </p>
              <p>
                Limits for age restrictions should use ‘aged [age] years and
                over’ or ‘aged under [age] years. Don’t use the plus sign. For
                example:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>aged 75 years and over</li>
                <li>aged under 18 years</li>
              </ul>
              <p>
                As per
                <a href="https://style.ons.gov.uk/category/house-style/numbers/#ages">
                  ONS style guide - Ages
                </a>
                .
              </p>
              <h3 id="ampersand">Ampersand</h3>
              <p>
                Use 'and' rather than & unless it’s in an organisation’s logo
                image or official name.
              </p>
              <p>
                The reason is that ‘and’ is easier to read. Some people with
                lower literacy levels also find ampersands harder to understand.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#ampersands-can-be-hard-to-understand">
                  GDS style guidance - Ampersands can be hard to understand
                </a>
                .
              </p>
              <h3 id="applied-general-qualifications">
                applied general qualifications
              </h3>
              <p>Lower case.</p>
              <h3 id="apprenticeship-programme">apprenticeship programme</h3>
              <p>Lower case.</p>
              <h3 id="autumn-census">autumn census</h3>
              <p>Lower case.</p>
            </AccordionSection>
            <AccordionSection heading="B">
              <h3 id="banned-words">Banned words</h3>
              <p>
                See <a href="#words-to-avoid">Words to avoid</a>
              </p>
              <h3 id="baseline">baseline</h3>
              <p>One word, lower case.</p>
              <h3 id="billions">Billions</h3>
              <p>
                See <a href="#millions-and-billions">Millions and billions</a>.
              </p>
              <h3 id="bold">bold</h3>
              <p>
                Use bold sparingly as using it too much makes it difficult for
                users to know which parts of your content they need to pay the
                most attention to.
              </p>
              <p>
                Before using bold, see if you can emphasise using your content
                in other ways by using bullets or subheadings.
              </p>
              <h3 id="borough-council">borough council</h3>
              <p>Lower case even in a name: Northampton borough council.</p>
              <h3 id="brackets">Brackets</h3>
              <p>Use (round brackets), not [square brackets]. </p>
              <p>
                Do not use round brackets to refer to something that could
                either be singular or plural, like ‘Check which document(s) you
                need to send to DfE.’
              </p>
              <p>
                Always use the plural instead as this covers each possibility.
                For example ‘Check which documents you need to send to DfE.’
              </p>
              <h3 id="britain">Britain</h3>
              <p>
                See <a href="#great-britain">Great Britain</a>
              </p>
              <h3 id="btec-national-diploma">BTEC National Diploma</h3>
              <p>Upper case.</p>
              <h3 id="bullet-points">Bullet points</h3>
              <p>
                Use bullet points to emphasise and make you content easier to
                read. However, the content needs to be structured so it’s
                suitable for use in a bulleted list so make sure:
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
                the lead text.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#bullet-points-steps ">
                  GDS style guide - Bullet points and steps
                </a>
                .
              </p>
            </AccordionSection>
            <AccordionSection heading="C">
              <h3 id="c-of-e">C of E</h3>
              <p>For Church of England when referring to school names.</p>
              <h3 id="cannot">cannot</h3>
              <p>Do not contract. Do not use ‘can’t’.</p>
              <p>
                See also <a href="#contractions">Contractions</a>.
              </p>
              <h3 id="capitalisation">Capitalisation</h3>
              <p>
                Always use lower case, even in page titles. The exceptions to
                this are proper nouns.
              </p>
              <p>
                For a full list of what you ‘do’ and ‘do not’ cap up -{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#capitalisation">
                  GDS style guide Capitalisation
                </a>
                .
              </p>
              <p>
                Capital letters are reputed to be 13 to 18% harder for users to
                read. So we try to avoid them.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#capital-letters-are-harder-to-read">
                  GDS style guidance - Capitals are harder to read
                </a>
                .
              </p>
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
              <h3 id="childcare">childcare</h3>
              <p>Lower case.</p>
              <h3 id="children-in-need">children in need</h3>
              <p>Lower case.</p>
              <h3 id="civil-service">Civil Service</h3>
              <p>Upper case.</p>
              <h3 id="civil-servants">civil servants</h3>
              <p>Lower case.</p>
              <h3 id="classwork">classwork</h3>
              <p>One word.</p>
              <h3 id="code-of-practice">code of practice</h3>
              <p>Lower case.</p>
              <h3 id="community-voluntary-and-foundation-schools">
                community, voluntary and foundation schools
              </h3>
              <p>Lower case.</p>
              <h3 id="contractions">Contractions</h3>
              <p>
                Use contractions as they make your content more conversational
                and engaging.
              </p>
              <p>
                Avoid can’t and don’t as many users find these hard to read or
                misread them as the opposite of what they say.
              </p>
              <p>
                Also avoid should’ve, could’ve, would’ve and they’ve as these
                can be hard to read.
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
              <p>Treat as a singular noun.</p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#data">
                  GDS style guide - data
                </a>
                .
              </p>
              <h3 id="data-centre">data centre</h3>
              <p>Not “datacentre”.</p>
              <h3 id="data-set">data set</h3>
              <p>Not “dataset”.</p>
              <h3 id="data-store">data store</h3>
              <p>Not “datastore”.</p>
              <h3 id="dates">Dates</h3>
              <p>
                Use the format ‘[Date] [Month] [Year]’ (depending on what
                information you have) written out with no commas. No ‘st’, ‘nd’,
                ‘rd’ and ‘th’.
              </p>
              <p>For example, 12 March 2019</p>
              <p>Use upper case for months: January, February.</p>
              <p>Do not use a comma between the month and year: 4 June 2017.</p>
              <p>
                When space is an issue - in tables or publication titles, for
                example - you can use truncated months: Jan, Feb.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#dates">
                  GDS style guide - Dates
                </a>{' '}
                and{' '}
                <a href="https://style.ons.gov.uk/category/house-style/numbers/#dates">
                  ONS style guide - Dates
                </a>
              </p>
              <h3 id="date-ranges-and-spans">Date ranges and spans</h3>
              <p>
                Use ‘to’ instead of a dash or slash in date ranges. ‘To’ is
                quicker to read than a dash, and it’s easier for screen readers.
              </p>
              <p>
                Always explain what date ranges represent. For example, academic
                year 2013 to 2014 or September 2013 to July 2014.
              </p>
              <p>
                Use the format ‘[date] to [date]’. If using months, repeat the
                year after each month if the period spans years.
              </p>
              <p>
                For example, 2018 to 2019, July to September 2019 or July 2018
                to September 2019
              </p>
              <p>
                For a period between two dates, use the format ‘between [date]
                and [date]’.
              </p>
              <p>
                For example, between 2010 and 2019 or between July and September
                2019.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#date-ranges">
                  GDS style guide - Dates
                </a>{' '}
                and{' '}
                <a href="https://style.ons.gov.uk/category/house-style/numbers/#dates">
                  ONS style guide - Dates
                </a>
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
              <h3 id="do-not">do not</h3>
              <p>Do not contract. Do not use ‘don’t’.</p>
              <p>
                See also <a href="#contractions">Contractions</a>.
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
              <h3 id="eg">eg</h3>
              <p>
                Do not use as can sometimes be read aloud as ‘egg’ by screen
                reading software.
              </p>
              <p>
                Use ‘for example’ or ‘such as’ or ‘like’ or ‘including’ -
                whichever works best in the specific context.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#eg-etc-and-ie">
                  GDS style guide - eg, etc and ie
                </a>
                .
              </p>
              <h3 id="email">email</h3>
              <p>One word.</p>
              <h3 id="email-addresses">Email addresses</h3>
              <p>
                Write in full, in lower case and as active links. Do not include
                any other words in the email’s link text.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#email-addresses">
                  GDS style guide - Email addresses
                </a>
                .
              </p>
              <h3 id="enrol">enrol</h3>
              <p>Lower case.</p>
              <h3 id="enrolling">enrolling</h3>
              <p>Lower case.</p>
              <h3 id="enrolment">enrolment</h3>
              <p>Lower case.</p>
              <h3 id="etc">Etc</h3>
              <p>
                Try to avoid and use ‘for example’ or ‘such as’ or ‘like’ or
                ‘including’ as per ‘eg’ above.
              </p>
              <p>
                Never use etc at the end of a list starting with these words.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#eg-etc-and-ie">
                  GDS style guide - eg, etc and ie
                </a>
                .
              </p>
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
              <h3 id="excel-spreadsheet">Excel spreadsheet</h3>
              <p>Upper case because Excel is a brand name.</p>
              <h3 id="exclusions">Exclusions</h3>
              <p>Lower case</p>
              <h3 id="executive-director">executive director</h3>
              <p>
                Lower case in text. Upper case in titles: Spencer Tracy,
                Executive Director, GDS.
              </p>
              <h3 id="extra-curricular">extra-curricular</h3>
              <p>Hyphenated</p>
            </AccordionSection>
            <AccordionSection heading="F">
              <h3 id="faqs-frequently-asked-questions">
                FAQs (frequently asked questions)
              </h3>
              <p>
                Do not use FAQs in any published releases. If you write content
                by starting with user needs, you will not need to use FAQs.
              </p>
              <p>
                As per
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#faqs-frequently-asked-questions">
                  GDS style guide - FAQs
                </a>
                and
                <a href="https://www.gov.uk/guidance/content-design/writing-for-gov-uk#do-not-use-faqs">
                  GDS style guidance - Do not use FAQs
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
              <h3 id="for-example">for example</h3>
              <p>Use as an alternative to eg.</p>
              <p>
                See also <a href="#eg">eg</a>.
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
              <h3 id="front-loading">Front-loading</h3>
              <p>
                Front-loading means putting the most important words and
                information at the beginning of your content whether that’s a
                heading, sentence or subheading.
              </p>
              <p>
                Doing this makes it easier and quicker for people to read and
                understand your content and the faster users can consume your
                content - the happier they’ll be.
              </p>
              <p>
                The earlier the most important words appear, the better. For
                example, which of these headings is quicker to understand?
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>What are the facts about education statistics?</li>
                <p className="govuk-!-font-size-19 govuk-!-font-weight-bold">
                  OR
                </p>
                <li>Education statistics: the facts</li>
              </ul>
              <p>
                Front-loading content can take some getting used to but it will
                help you create clearer, more concise content.
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
                  The Guardian style guide
                </a>{' '}
                for advice on hyphenation.
              </p>
              <p>
                Use ‘to’ for <a href="#time">time</a> and{' '}
                <a href="#date-ranges-and-spans">date ranges and spans</a> - not
                hyphens.
              </p>
            </AccordionSection>
            <AccordionSection heading="I">
              <h3 id="ie">ie</h3>
              <p>
                Try (re)writing sentences to avoid the need to use it. If that
                is not possible, use an alternative such as ‘meaning’ or ‘that
                is’.
              </p>
              <p>
                Can be used within service content to save space but write
                without including full stops. For example, ie NOT i.e.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#eg-etc-and-ie">
                  GDS style guide - eg, etc and ie
                </a>
                .
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
              <h3 id="jargon">Jargon</h3>
              <p>
                Do not use jargon. We want to introduce education statistics to
                new people so we need to write for a non-expert audience.
              </p>
              <p>
                Research has also proven that even experts do not have time or
                want to wade through jargon on web pages.
              </p>
              <p>
                Instead, they want information quickly and easily - just like
                everyone else.
              </p>
              <p>
                So by not using jargon, you’re not ‘dumbing down’. You’re
                actually ‘opening up’ to all your levels of users.
              </p>
              <p>
                However, there will be times when you need to introduce users to
                a new concept, phrase or term.
              </p>
              <p>
                In these cases, provide them with a short contextual explanation
                the first time you mention it within content and then use a
                ‘glossary’ link to explain things in greater depth.
              </p>
              <p>
                Alternatively, you can provide users with a link to a full
                explanation in a methodology document.
              </p>
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
                <h3 id="key-indicators">Key indicators</h3>
                <p>
                  Our research has shown you should aim to add up to a maximum
                  of 6 key indicators within the ‘Headline facts and figures’
                  section of a release.
                </p>
                <p>
                  You’ll also need to add content to the ‘Guidance title’ and
                  ‘Guidance text’ text boxes within the Admin system to explain
                  what each key indicator means.
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
                active and specific.
              </p>
              <p>
                Always link to online services first and offer offline
                alternatives afterwards.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#links">
                  GDS style guide - Links
                </a>
                .
              </p>
              <h3 id="lists">Lists</h3>
              <p>Lists should be bulleted to make them easier to read.</p>
              <p>
                <a href="#bullet-points">See Bullet points</a>
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
              <h3 id="metadata">metadata</h3>
              <p>Not “meta data”.</p>
              <h3 id="middle-deemed-secondary-schools">
                middle-deemed secondary schools
              </h3>
              <p>Lower case. Hyphenated.</p>
              <h3 id="millions-and-billions">Millions and billions</h3>
              <p>Always use million in money (and billion): £138 million.</p>
              <p>Use millions in phrases: millions of people.</p>
              <p>
                But do not use £0.xx million for amounts less than £1 million.
              </p>
              <p>Do not abbreviate million to m.</p>
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
              <h3 id="national-statistics">National Statistics</h3>
              <p>
                Upper case National Statistics for the official statistics
                quality mark.
              </p>
              <p>
                Lower case for anything else, including statistics that are
                national in scope.
              </p>
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
              <p>Write all numbers 10 and over as numerals, up to 999,999.</p>
              <p>
                Write numbers one to nine as words unless they are technical or
                precise, such as dates, figure or table titles, or relate
                directly to the statistics being presented.
              </p>
              <p>For example:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>On the one hand…</li>
                <li>This is the most effective of the two measures…</li>
                <li>7 March 2017</li>
                <li>1,000</li>
                <li>Figure 1</li>
              </ul>
              <p>
                Where a range crosses the 10 boundary, use numerals. For
                example, 9 to 12 respondents, not nine to 12 respondents.
              </p>
              <p>
                Write out rankings first to ninth, then use numerals. Do not use
                superscript for ‘st’, ‘nd’, ‘rd’ and ‘th’. For example, first or
                10th.
              </p>
              <p>
                A sequence of numbers should use the same format for both, which
                should follow the higher number. For example, 6th out of 12. Do
                not use abbreviations of ‘numbers’, such as ‘no’ or ‘nos’. They
                can be read incorrectly.
              </p>
              <p>
                Use commas after every three decimal places in numbers of four
                digits or more - never spaces. Years should have no punctuation.
                For example:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>100,000</li>
                <li>2,548</li>
                <li>1995</li>
              </ul>
              <p>Avoid writing sets of numbers together. For example:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>‘In 1961 just over 2,500 births were recorded.’</li>
                <p className="govuk-!-font-size-19 govuk-!-font-weight-bold">
                  NOT
                </p>
                <li>‘In 1961 2,543 births were recorded.’</li>
              </ul>
              <p>
                Use a 0 where there’s no digit before the decimal point in a
                number. For example, 0.6%.
              </p>
              <p>
                Do not start a sentence with a numeral. Rearrange the sentence
                accordingly. For example:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>‘There are 10 million pupils in school in England.’</li>
                <p className="govuk-!-font-size-19 govuk-!-font-weight-bold">
                  NOT
                </p>
                <li>‘10 million pupils go to school in England.’</li>
              </ul>
              <p>
                Do not use a hyphen to indicate a range of numbers - use ‘to’
                instead. For example, ‘Around 150 to 200 pupils attended the
                school’.
              </p>
              <p>
                As per{' '}
                <a href="https://style.ons.gov.uk/category/house-style/numbers/#writing-numbers">
                  ONS style guide - Writing numbers
                </a>
                .
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
              <h3 id="overseas-trained-teacher">overseas-trained teacher</h3>
              <p>Lower case. Hyphenated.</p>
            </AccordionSection>
            <AccordionSection heading="P">
              <h3 id="page-length">Page length</h3>
              <p>There is no minimum or maximum page length for a release.</p>
              <p>However, research has shown:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>people only read 20 to 28% of web pages anyway</li>
                <li>
                  that the pressure on people to understand increases for every
                  100 words added to web pages
                </li>
              </ul>
              <p>
                This means the quicker you get to the point, the greater chance
                users have of understanding the content you’ve created.
              </p>
              <p>
                We suggest you limit your word count per section of your release
                to 250 words with an absolute maximum of 400 words if you have a
                lot of information to put across.
              </p>
              <p>
                In cases where your word count rises about 250 words - make sure
                you use bullets and subheadings to break up your content and
                make it easier to read and understand.
              </p>
              <p>
                Alternatively, see if you can break up long sections like these
                into 2 different sections by grouping the content into
                like-minded themes and topics.
              </p>
              <h3 id="paper-b">Paper B</h3>
              <p>In national curriculum tests.</p>
              <h3 id="parliament">Parliament</h3>
              <p>Upper case.</p>
              <h3 id="parliamentary-committees">Parliamentary committees</h3>
              <p>
                Parliamentary is upper case and committees is in lower case.
              </p>
              <h3 id="parliamentary-questions">parliamentary questions</h3>
              <p>Lower case.</p>
              <h3 id="pathfinder">pathfinder</h3>
              <p>Lower case.</p>
              <h3 id="pdf">PDF</h3>
              <p>Upper case. No need to explain the acronym.</p>
              <h3 id="per-cent">Per cent</h3>
              <p>
                Two words and NOT percent. Always use % with a number.
                Percentage is one word.
              </p>
              <p>
                As per{' '}
                <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#per-cent">
                  GDS style guide - per cent
                </a>
                .
              </p>
              <h3 id="performance-management">performance management</h3>
              <p>Lower case.</p>
              <h3 id="performance-tables">performance tables</h3>
              <p>Lower case.</p>
              <h3 id="physical-education-or-pe">physical education or PE</h3>
              <p>You can write in full or use the initials.</p>
              <h3 id="plain-english">plain English</h3>
              <p>
                Do not use formal or long words when easy or short ones will do.
              </p>
              <p>
                Users have a primary set of vocabulary of 5,000 common words
                which most use every day so try and use these to get your point
                across.
              </p>
              <p>
                See
                <a href="#words-to-avoid">Words to avoid</a>.
              </p>
              <h3 id="policy-note">policy note</h3>
              <p>Lower case.</p>
              <h3 id="policy-statement">policy statement</h3>
              <p>Lower case.</p>
              <h3 id="postcode">postcode</h3>
              <p>All one word.</p>
              <h3 id="powerpoint-presentation">PowerPoint presentation</h3>
              <p>Upper case because PowerPoint is a brand name.</p>
              <h3 id="pre-school">pre-school</h3>
              <p>Hyphenated.</p>
              <h3 id="prime-minister">Prime Minister</h3>
              <p>Use Prime Minister Theresa May and the Prime Minister.</p>
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
              <h3 id="pupil-premium">pupil premium</h3>
              <p>Lower case.</p>
              <h3 id="punctuation">Punctuation</h3>
              <p>
                Stick to using familiar punctuation like commas, dashes and full
                stops.
              </p>
              <p>
                This is because most people do not know how to use little-used
                punctuation such as square brackets and semicolons.
              </p>
              <p>
                Using more complicated punctuation slows people down and this in
                turn makes your content harder to understand.
              </p>
              <p>
                Simple and familiar punctuation aids a user’s reading speed and
                comprehension.
              </p>
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
            </AccordionSection>
            <AccordionSection heading="S">
              <h3 id="same-sex-schools">same-sex schools</h3>
              <p>Hyphenated.</p>
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
              <h3 id="school-census">school census</h3>
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
                Do not use long sentences. Sentences should be no longer than 25
                words.
              </p>
              <p>
                If they’re any longer then they need to be divided into two.
              </p>
              <p>
                Say what you need to say once, clearly and keep sentences to one
                or two tightly-connected thoughts.
              </p>
              <p>If you have another thought - put it in another sentence.</p>
              <p>
                As per{' '}
                <a href="https://style.ons.gov.uk/category/writing-for-the-web/structuring-content/#sentences">
                  ONS style guide - Sentences
                </a>
                .
              </p>
              <h3 id="short-sentences">Short sentences</h3>
              <p>
                See <a href="#sentence-length">Sentence length</a>.
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
              <h3 id="simple-language">Simple language</h3>
              <p>
                See <a href="#plain-english">plain English</a>.
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
              <h3 id="spads">SPAds</h3>
              <p>
                Not SPADs which stands for ‘signals passed at danger’ which is a
                government transport term.
              </p>
              <h3 id="spaces">spaces</h3>
              <p>One space after a full stop, not 2.</p>
              <h3 id="special-advisers">special advisers</h3>
              <p>Lower case.</p>
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
              <h3 id="spring-census">spring census</h3>
              <p>Lower case.</p>
              <h3 id="standards-of-conduct">standards of conduct</h3>
              <p>Lower case.</p>
              <h3 id="statement-of-send">statement of SEND</h3>
              <p>Lower case.</p>
              <h3 id="statistical-first-release">statistical first release</h3>
              <p>Lower case.</p>
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
              <h3 id="subheadings">Subheadings</h3>
              <p>
                Using subheadings to break up your content on a web pages makes
                it much easier for people to take in the information it
                contains.
              </p>
              <p>
                They help by telling a story across your page and people use
                them to predict in seconds what information is on any particular
                part of your page.
              </p>
              <p>
                Subheadings make reading content easier by acting as markers
                which people can return to if they need to re-read something to
                aid their understanding.
              </p>
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
              <h3 id="summer-census">summer census</h3>
              <p>Lower case.</p>
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
              <h3 id="underperforming">underperforming</h3>
              <p>One word.</p>
              <h3 id="unique-pupil-number">unique pupil number</h3>
              <p>Lower case.</p>
              <h3 id="university-technical-college">
                university technical college
              </h3>
              <p>Lower case.</p>
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
