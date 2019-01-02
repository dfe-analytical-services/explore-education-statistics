import React, { Component } from 'react';
import axios from 'axios';
import DataList from '../components/datalist';
import Title from '../components/title';

class Theme extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        const { handle } = this.props.match.params;
        axios.get(`http://localhost:5010/api/Theme/${handle}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        const topics = [{
            id: "1003fa5c-b60a-4036-a178-e3a69a81b852",
            title: "Absence and Exclusions"
        }];
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label={data.title} />
                    <h2>What sort of stats are you looking for?</h2>
                    <DataList data={topics} linkIdentifier='topic' />
              </div>
            </div>
        );
    }
}

export default Theme;