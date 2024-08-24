import { GenerateStatistic } from "./generate-statistic.model";

export interface GenerateResult {
    generateTime: number;
    executionTime: number;
    passwords: string[];
    statistics: GenerateStatistic[];
  }