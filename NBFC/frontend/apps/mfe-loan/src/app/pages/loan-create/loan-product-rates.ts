import { LoanProductInfo, LoanProductType } from '@patsanstha/loans-data-access';

/** Local types/helpers — avoids native-federation re-export issues from shared libs. */
interface LoanProductApiDto {
  productType: LoanProductType;
  defaultInterestRatePercent: number;
}

export function mergeLoanProductRates(
  catalog: LoanProductInfo[],
  apiProducts: LoanProductApiDto[]
): LoanProductInfo[] {
  return catalog.map((product) => {
    const fromApi = apiProducts.find((item) => item.productType === product.productType);
    return fromApi
      ? { ...product, rate: Number(fromApi.defaultInterestRatePercent) }
      : product;
  });
}
