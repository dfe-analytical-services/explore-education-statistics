import React, { Component } from 'react';
import DataList from '../components/datalist';
import Title from '../components/title';
import API from '../api';
import Breadcrumbs from '../components/breadcrumbs';
import Glink from '../components/glink';

class Theme extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: [],
            topics: []
        }
    }

    componentDidMount() {
        const { handle } = this.props.match.params;
        API.get(`theme/${handle}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
        API.get(`theme/${handle}/topics`)
            .then(json => this.setState({ topics: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        const { topics } = this.state;
        return (
            <div>
                <Breadcrumbs />
                <div className="govuk-grid-row">
                    <div className="govuk-grid-column-two-thirds">
                        <Title label={data.title} />
                        <h2 className="govuk-heading-l">Find school statistics</h2>
                        <p class="govuk-body">Here you can ind DfE stats for Schools, and access them as reports, customise and download as excel files or csv files, and access them via an API. <Glink>(Find out more)</Glink></p>
                        <p class="govuk-body">You can also see our statistics for 16+ education and social care.</p>
                        <div class="govuk-form-group">
                            <label class="govuk-label" for="find-for-schools">
                                Find any DfE statistic, publication or indicator for schools:
                            </label>
                            <span id="find-for-schools" class="govuk-hint">
                                Find publications, statistics and indicators for Schools.
                            </span>
                            <input class="govuk-input govuk-input--width-40" id="find-for-schools" name="find-for-schools" type="text" aria-describedby="find-for-schools-hint" />
                            <button type="submit" class="govuk-button">
                                Find
                            </button>
                        </div>
                        <h3 class="govuk-heading-m">What sort of stats are you looking for?</h3>
                        <DataList data={topics} linkIdentifier='topic' />
                        <hr class="govuk-section-break--l govuk-section-break--visible" />
                        <h2 class="govuk-heading-l">Latest publications in Schools <Glink>(see all school publications)</Glink></h2>
                        <p class="govuk-body">These are the latest official statistics with figures in schools. You can access the report and commentary, and also get the data for use in Excel and other tools. You can now customise the data to your requirements, and get a variety of formats. <Glink>(Find out more)</Glink> <Glink>(Find more publications)</Glink></p>
                        <hr class="govuk-section-break--l govuk-section-break--visible" />
                        <h2 class="govuk-heading-l">Key indicators for Schools <Glink>(change)</Glink></h2>
                        <p class="govuk-body">These are some key indicators for Schools. You can change what you see here according to your requirements. <Glink>(Find out more)</Glink></p>
                        <hr class="govuk-section-break--l govuk-section-break--visible" />
                        <h2 class="govuk-heading-l">Explore School statistics</h2>
                        <ul class="govuk-list govuk-list--bullet">
                            <li>You can explore all the DfE statistics available for schools here. You can use our step by step guide, or dive straight in.</li>
                            <li>Once you've chosen your data you can view it by year, school type, area or pupic characteristics.</li>
                            <li>You can also dowload it, visualise it or copy and paste it as you need.</li>
                        </ul>
                        <div class="govuk-form-group">
                            <fieldset class="govuk-fieldset">
                                <legend class="govuk-fieldset__legend govuk-fieldset__legend--m">
                                <h1 class="govuk-fieldset__heading">
                                    Want to explore more school stats? What do you want to do?
                                </h1>
                                </legend>
                                <div class="govuk-radios">
                                    <div class="govuk-radios__item">
                                        <input class="govuk-radios__input" id="what-to-do-1" name="what-to-do" type="radio" value="see-how-stats-have-changed" />
                                        <label class="govuk-label govuk-radios__label" for="what-to-do-1">
                                            See how school stats have changed over time for <Glink>(choose)</Glink>
                                        </label>
                                    </div>
                                    <div class="govuk-radios__item">
                                        <input class="govuk-radios__input" id="what-to-do-2" name="what-to-do" type="radio" value="compare-stats-over-time" />
                                        <label class="govuk-label govuk-radios__label" for="what-to-do-2">
                                            Compare school stats over time to <Glink>(choose)</Glink>
                                        </label>
                                    </div>
                                    <div class="govuk-radios__item">
                                        <input class="govuk-radios__input" id="what-to-do-3" name="what-to-do" type="radio" value="compare-current-stats-with" />
                                        <label class="govuk-label govuk-radios__label" for="what-to-do-3">
                                            Compare school current stats to <Glink>(choose)</Glink>
                                        </label>
                                    </div>
                                    <div class="govuk-radios__item">
                                        <input class="govuk-radios__input" id="what-to-do-4" name="what-to-do" type="radio" value="find-specific-stats" />
                                        <label class="govuk-label govuk-radios__label" for="what-to-do-4">
                                            Find specific stats for <Glink>(choose)</Glink>
                                        </label>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <hr class="govuk-section-break--l govuk-section-break--visible" />
                        <h2 class="govuk-heading-l">Find and compare stats for schools</h2>
                    </div>
                </div>
            </div>
        );
    }
}

export default Theme;